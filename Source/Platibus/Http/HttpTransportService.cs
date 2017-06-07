// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Common.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Platibus.Journaling;

namespace Platibus.Http
{
    /// <summary>
    /// An <see cref="ITransportService"/> that uses the HTTP protocol to transmit messages
    /// between Platibus instances
    /// </summary>
    public class HttpTransportService : ITransportService, IQueueListener, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);

        private readonly Uri _baseUri;
        private readonly IEndpointCollection _endpoints;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly bool _bypassTransportLocalDestination;
        private readonly Func<Message, CancellationToken, Task> _handleMessage;
        private readonly QueueName _outboundQueueName;

        private readonly HttpClientPool _clientPool;

        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="HttpTransportService"/>
        /// </summary>
        /// <param name="baseUri">The base URI of the local Platibus instance</param>
        /// <param name="endpoints">The configured endpoints for the local Platibus instance</param>
        /// <param name="messageQueueingService">The service used to queue outbound messages</param>
        /// <param name="messageJournal">A log of events service used to track when messages are
        /// sent</param>
        /// <param name="subscriptionTrackingService">The service used to track subscriptions to
        /// local topics</param>
        /// <param name="bypassTransportLocalDestination">Whether to bypass HTTP transport and
        /// instead pass messages to the supplied <paramref name="handleMessage"/> delegate for
        /// messages whose destinations are equal to the <paramref name="baseUri"/></param>
        /// <param name="handleMessage">A delegate used to handle messages locally rather than
        /// incurring the costs of sending them over HTTP if 
        /// <paramref name="bypassTransportLocalDestination"/> is <c>true</c></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseUri"/>, 
        /// <paramref name="messageQueueingService"/>, or <paramref name="subscriptionTrackingService"/>
        /// are <c>null</c></exception>
        public HttpTransportService(Uri baseUri, IEndpointCollection endpoints, IMessageQueueingService messageQueueingService, IMessageJournal messageJournal, ISubscriptionTrackingService subscriptionTrackingService, bool bypassTransportLocalDestination = false, Func<Message, CancellationToken, Task> handleMessage = null)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (messageQueueingService == null) throw new ArgumentNullException("messageQueueingService");
            if (subscriptionTrackingService == null) throw new ArgumentNullException("subscriptionTrackingService");
            if (bypassTransportLocalDestination && handleMessage == null)
            {
                throw new ArgumentNullException("handleMessage", "A message handler is required if bypassTransportLocalDestination is true");
            }

            _baseUri = baseUri.WithTrailingSlash();
            _endpoints = endpoints == null
                ? ReadOnlyEndpointCollection.Empty
                : new ReadOnlyEndpointCollection(endpoints);

            _messageQueueingService = messageQueueingService;
            _messageJournal = messageJournal;
            _subscriptionTrackingService = subscriptionTrackingService;
            _bypassTransportLocalDestination = bypassTransportLocalDestination;
            _handleMessage = handleMessage;
            _outboundQueueName = "Outbound";

            _clientPool = new HttpClientPool();
        }

        /// <summary>
        /// Performs final initialization of the transport service
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// can use to cancel the initialization process</param>
        /// <returns>Returns a task that completes when initialization is complete</returns>
        public async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageQueueingService.CreateQueue(_outboundQueueName, this, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Handles a message that is received off of a queue
        /// </summary>
        /// <param name="message">The message that was received</param>
        /// <param name="context">The context in which the message was dequeued</param>
        /// <param name="cancellationToken">A cancellation token provided by the
        /// queue that can be used to cancel message processing</param>
        /// <returns>Returns a task that completes when the message is processed</returns>
        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            IEndpoint endpoint;
            IEndpointCredentials credentials = null;
            if (_endpoints.TryGetEndpointByAddress(message.Headers.Destination, out endpoint))
            {
                credentials = endpoint.Credentials;
            }
            cancellationToken.ThrowIfCancellationRequested();
            Thread.CurrentPrincipal = context.Principal;
            await TransportMessage(message, credentials, cancellationToken);
            await context.Acknowledge();
        }

        /// <summary>
        /// Sends a message directly to the application identified by the
        /// <see cref="IMessageHeaders.Destination"/> header.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="credentials">The credentials required to send a 
        /// message to the specified destination, if applicable.</param>
        /// <param name="cancellationToken">A token used by the caller to
        /// indicate if and when the send operation has been canceled.</param>
        /// <returns>returns a task that completes when the message has
        /// been successfully sent to the destination.</returns> 
        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Headers.Destination == null) throw new ArgumentException("Message has no destination");

            if (message.Headers.Importance == MessageImportance.Critical)
            {
                Log.DebugFormat("Enqueueing critical message ID {0} for queued outbound delivery...", message.Headers.MessageId);
                await _messageQueueingService.EnqueueMessage(_outboundQueueName, message, Thread.CurrentPrincipal, cancellationToken);
                return;
            }

            await TransportMessage(message, credentials, cancellationToken);
        }

        /// <summary>
        /// Publishes a message to a topic.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="topicName">The name of the topic.</param>
        /// <param name="cancellationToken">A token used by the caller
        /// to indicate if and when the publish operation has been canceled.</param>
        /// <returns>returns a task that completes when the message has
        /// been successfully published to the topic.</returns>
        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            var subscribers = await _subscriptionTrackingService.GetSubscribers(topicName, cancellationToken);
            var transportTasks = new List<Task>();

            foreach (var subscriber in subscribers)
            {
                IEndpointCredentials subscriberCredentials = null;
                IEndpoint subscriberEndpoint;
                if (_endpoints.TryGetEndpointByAddress(subscriber, out subscriberEndpoint))
                {
                    subscriberCredentials = subscriberEndpoint.Credentials;
                }

                var perEndpointHeaders = new MessageHeaders(message.Headers)
                {
                    MessageId = MessageId.Generate(),
                    Destination = subscriber
                };

                var addressedMessage = new Message(perEndpointHeaders, message.Content);
                if (addressedMessage.Headers.Importance == MessageImportance.Critical)
                {
                    Log.DebugFormat("Enqueueing critical message ID {0} for queued outbound delivery...", message.Headers.MessageId);
                    await _messageQueueingService.EnqueueMessage(_outboundQueueName, addressedMessage, null, cancellationToken);
                    continue;
                }

                Log.DebugFormat("Forwarding message ID {0} published to topic {0} to subscriber {2}...", message.Headers.MessageId, topicName, subscriber);
                transportTasks.Add(TransportMessage(addressedMessage, subscriberCredentials, cancellationToken));
            }

            await Task.WhenAll(transportTasks);
        }
        
        private async Task TransportMessage(Message message, IEndpointCredentials credentials, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpClient httpClient = null;
            try
            {
                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Sent, cancellationToken);
                }

                var endpointBaseUri = message.Headers.Destination.WithTrailingSlash();
                if (_bypassTransportLocalDestination && endpointBaseUri == _baseUri)
                {
                    Log.DebugFormat("Handling local delivery of message ID {0} to destination {1}...", message.Headers.MessageId, message.Headers.Destination);
                    await _handleMessage(message, cancellationToken);
                    return;
                }

                var httpContent = new StringContent(message.Content);
                WriteHttpContentHeaders(message, httpContent);
                
                httpClient = await _clientPool.GetClient(endpointBaseUri, credentials, cancellationToken);
                var messageId = message.Headers.MessageId;
                var urlEncodedMessageId = HttpUtility.UrlEncode(messageId);
                var relativeUri = string.Format("message/{0}", urlEncodedMessageId);
                var postUri = new Uri(endpointBaseUri, relativeUri);
                Log.DebugFormat("POSTing content of message ID {0} to URI {1}...", message.Headers.MessageId, postUri);
                var httpResponseMessage = await httpClient.PostAsync(relativeUri, httpContent, cancellationToken);
                Log.DebugFormat("Received HTTP response code {0} {1} for POST request {2}...",
                    (int) httpResponseMessage.StatusCode,
                    httpResponseMessage.StatusCode.ToString("G"),
                    postUri);

                HandleHttpErrorResponse(httpResponseMessage);
            }
            catch (TransportException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (MessageNotAcknowledgedException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Error sending message ID {0}", message.Headers.MessageId);
                Log.ErrorFormat(errorMessage, ex);

                HandleCommunicationException(ex, message.Headers.Destination);

                throw new TransportException(errorMessage, ex);
            }
            finally
            {
                httpClient.TryDispose();
            }
        }

        /// <summary>
        /// Subscribes to messages published to the specified <paramref name="topicName"/>
        /// by the application at the provided <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="topicName">The name of the topic to which the caller is
        ///     subscribing.</param>
        /// <param name="ttl">(Optional) The Time To Live (TTL) for the subscription
        ///     on the publishing application if it is not renewed.</param>
        /// <param name="cancellationToken">A token used by the caller to
        ///     indicate if and when the subscription should be canceled.</param>
        /// <returns>Returns a long-running task that will be completed when the 
        /// subscription is canceled by the caller or a non-recoverable error occurs.</returns>
        public async Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var subscription = new HttpSubscriptionMetadata(endpoint, topicName, ttl);
                while (!cancellationToken.IsCancellationRequested)
                {
                    TimeSpan retryOrRenewAfter;
                    try
                    {
                        Log.DebugFormat("Sending subscription request for topic {0} to {1}...", topicName, endpoint);
                        await SendSubscriptionRequest(subscription, cancellationToken);
                        if (!subscription.Expires)
                        {
                            // Subscription is not set to expire on the remote server.  Since
                            // publications are pushed to the subscribers, we don't need to keep
                            // this task running.
                            return;
                        }
                        
                        retryOrRenewAfter = subscription.RenewalInterval;
                        Log.DebugFormat(
                            "Subscription request for topic {0} successfuly sent to {1}.  Subscription TTL is {2} and is scheduled to auto-renew in {3}",
                            topicName, endpoint, ttl, retryOrRenewAfter);
                    }
                    catch (EndpointNotFoundException enfe)
                    {
                        // Endpoint is not defined in the supplied configuration,
                        // so we cannot determine the URI.  This is an unrecoverable
                        // error, so simply return.
                        Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint \"{1}\"", enfe, topicName,
                            endpoint);
                        return;
                    }
                    catch (NameResolutionFailedException nrfe)
                    {
                        // The transport was unable to resolve the hostname in the
                        // endpoint URI.  This may or may not be a temporary error.
                        // In either case, retry after 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", nrfe,
                            topicName, endpoint, retryOrRenewAfter);
                    }
                    catch (ConnectionRefusedException cre)
                    {
                        // The transport was unable to resolve the hostname in the
                        // endpoint URI.  This may or may not be a temporary error.
                        // In either case, retry after 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", cre,
                            topicName, endpoint, retryOrRenewAfter);
                    }
                    catch (ResourceNotFoundException ire)
                    {
                        // Topic is not found.  This may be temporary.
                        retryOrRenewAfter = subscription.RetryInterval;
                        Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", ire,
                            topicName, endpoint, retryOrRenewAfter);
                    }
                    catch (InvalidRequestException ire)
                    {
                        // Request is not valid.  Either the URL is malformed or the
                        // topic does not exist.  In any case, retrying would be
                        // fruitless, so just return.
                        Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint {1}", ire, topicName,
                            endpoint);
                        return;
                    }
                    catch (TransportException te)
                    {
                        // Unspecified transport error.  This may or may not be
                        // due to temporary conditions that will resolve 
                        // themselves.  Retry in 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", te,
                            topicName, endpoint, retryOrRenewAfter);
                    }
                    await Task.Delay(retryOrRenewAfter, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task SendSubscriptionRequest(HttpSubscriptionMetadata subscriptionMetadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (subscriptionMetadata == null) throw new ArgumentNullException("subscriptionMetadata");

            var endpoint = subscriptionMetadata.Endpoint;
            var topic = subscriptionMetadata.Topic;
            var ttl = subscriptionMetadata.TTL;

            HttpClient httpClient = null;
            try
            {
                var endpointBaseUri = endpoint.Address.WithTrailingSlash();
                httpClient = await _clientPool.GetClient(endpointBaseUri, endpoint.Credentials, cancellationToken);

                var urlSafeTopicName = HttpUtility.UrlEncode(topic);
                var relativeUri = string.Format("topic/{0}/subscriber?uri={1}", urlSafeTopicName, _baseUri);
                if (ttl > TimeSpan.Zero)
                {
                    relativeUri += "&ttl=" + ttl.TotalSeconds;
                }

                var httpResponseMessage = await httpClient.PostAsync(relativeUri, new StringContent(""), cancellationToken);
                HandleHttpErrorResponse(httpResponseMessage);
            }
            catch (TransportException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Error sending subscription request for topic {0} of publisher {1}",
                    topic, endpoint.Address);
                Log.ErrorFormat(errorMessage, ex);

                HandleCommunicationException(ex, endpoint.Address);

                throw new TransportException(errorMessage, ex);
            }
            finally
            {
                httpClient.TryDispose();
            }
        }

        private static void HandleHttpErrorResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var statusCode = (int) response.StatusCode;
            var statusDescription = response.ReasonPhrase;

            if (statusCode == 401)
            {
                throw new UnauthorizedAccessException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
            }

            if (statusCode == 422)
            {
                throw new MessageNotAcknowledgedException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
            }

			if (statusCode == 404)
			{
				throw new ResourceNotFoundException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
			}

            if (statusCode < 500)
            {
                // Other HTTP 400-499 status codes identify invalid requests (bad request, authentication required, 
				// not authorized, etc.)
                throw new InvalidRequestException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
            }

            // HTTP 500+ are internal server errors
            throw new TransportException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
        }

        private static void HandleCommunicationException(Exception ex, Uri uri)
        {
            var handled = false;
            while (!handled)
            {
                var hre = ex as HttpRequestException;
                if (hre != null && hre.InnerException != null)
                {
                    ex = hre.InnerException;
                    continue;
                }

                var we = ex as WebException;
                if (we != null)
                {
                    switch (we.Status)
                    {
                        case WebExceptionStatus.NameResolutionFailure:
                            throw new NameResolutionFailedException(uri.Host);
                        case WebExceptionStatus.ConnectFailure:
                            throw new ConnectionRefusedException(uri.Host, uri.Port, ex.InnerException ?? ex);
                    }
                }

                var se = ex as SocketException;
                if (se != null)
                {
                    switch (se.SocketErrorCode)
                    {
                        case SocketError.ConnectionRefused:
                            throw new ConnectionRefusedException(uri.Host, uri.Port, ex.InnerException ?? ex);
                    }
                }

                handled = true;
            }
        }

        private static void WriteHttpContentHeaders(Message message, HttpContent content)
        {
            foreach (var header in message.Headers)
            {
                if ("Content-Type".Equals(header.Key, StringComparison.OrdinalIgnoreCase))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(header.Value);
                    continue;
                }

                content.Headers.Add(header.Key, header.Value);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether this method was called from the
        /// <see cref="Dispose()"/> method.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_clientPool")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clientPool.TryDispose();
            }
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}