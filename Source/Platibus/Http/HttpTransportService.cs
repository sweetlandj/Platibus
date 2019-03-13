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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.Utils;

namespace Platibus.Http
{
    /// <inheritdoc cref="ITransportService"/>
    /// <inheritdoc cref="IQueueListener"/>
    /// <inheritdoc cref="IDisposable"/>
    /// <summary>
    /// An <see cref="T:Platibus.ITransportService" /> that uses the HTTP protocol to transmit messages
    /// between Platibus instances
    /// </summary>
    public class HttpTransportService : ITransportService, IQueueListener, IDisposable
    {
        private static readonly UrlEncoder UrlEncoder = new UrlEncoder();

        private readonly Uri _baseUri;
        private readonly IEndpointCollection _endpoints;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly bool _bypassTransportLocalDestination;
        private readonly IDiagnosticService _diagnosticService;
        private readonly QueueName _outboundQueueName;
        private readonly IHttpClientFactory _httpClientFactory;

        private int _initialized;
        private bool _disposed;

        /// <inheritdoc />
        public event TransportMessageEventHandler MessageReceived;

        /// <summary>
        /// Initializes a new <see cref="HttpTransportService"/>
        /// </summary>
        /// <param name="options">The HTTP transport service configuration</param>
        public HttpTransportService(HttpTransportServiceOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _baseUri = options.BaseUri.WithTrailingSlash();
            _endpoints = options.Endpoints = options.Endpoints ?? EndpointCollection.Empty;

            _messageQueueingService = options.MessageQueueingService;
            _messageJournal = options.MessageJournal;
            _subscriptionTrackingService = options.SubscriptionTrackingService;
            _bypassTransportLocalDestination = options.BypassTransportLocalDestination;
            _diagnosticService = options.DiagnosticService ?? DiagnosticService.DefaultInstance;
            _httpClientFactory = options.HttpClientFactory ?? new BasicHttpClientFactory();
            _outboundQueueName = "Outbound";
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Http.HttpTransportService" />
        /// </summary>
        /// <param name="baseUri">The base URI of the local Platibus instance</param>
        /// <param name="endpoints">The configured endpoints for the local Platibus instance</param>
        /// <param name="messageQueueingService">The service used to queue outbound messages</param>
        /// <param name="messageJournal">A log of events service used to track when messages are
        /// sent</param>
        /// <param name="subscriptionTrackingService">The service used to track subscriptions to
        /// local topics</param>
        /// <param name="bypassTransportLocalDestination">Whether to bypass HTTP transport and
        /// instead raise the <see cref="E:Platibus.Http.HttpTransportService.LocalDelivery" /> event for messages whose destinations are 
        /// equal to the <paramref name="baseUri" /></param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="baseUri" />, 
        /// <paramref name="messageQueueingService" />, or <paramref name="subscriptionTrackingService" />
        /// are <c>null</c></exception>
        [Obsolete("Use HttpTransportService(HttpTransportServiceOptions options)")]
        public HttpTransportService(Uri baseUri, IEndpointCollection endpoints, 
            IMessageQueueingService messageQueueingService, IMessageJournal messageJournal, 
            ISubscriptionTrackingService subscriptionTrackingService,
            bool bypassTransportLocalDestination = false, 
            IDiagnosticService diagnosticService = null)
        : this(new HttpTransportServiceOptions(baseUri, messageQueueingService, subscriptionTrackingService)
        {
            DiagnosticService = diagnosticService,
            Endpoints = endpoints,
            MessageJournal = messageJournal,
            BypassTransportLocalDestination = bypassTransportLocalDestination,
            HttpClientFactory = new BasicHttpClientFactory()
        })
        {
        }

        /// <summary>
        /// Performs final initialization of the transport service
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// can use to cancel the initialization process</param>
        /// <returns>Returns a task that completes when initialization is complete</returns>
        public async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 0)
            {
                try
                {
                    await _messageQueueingService.CreateQueue(_outboundQueueName, this,
                        cancellationToken: cancellationToken);

                    await _diagnosticService.EmitAsync(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitialization)
                        {
                            Detail = "HTTP transport service initialized"
                        }.Build(), cancellationToken);
                }
                catch (Exception ex)
                {
                    _diagnosticService.Emit(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.ComponentInitializationError)
                        {
                            Detail = "Error initializating outbound transport queue",
                            Exception = ex
                        }.Build());
                    throw;
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Handles a message that is received off of a queue
        /// </summary>
        /// <param name="message">The message that was received</param>
        /// <param name="context">The context in which the message was dequeued</param>
        /// <param name="cancellationToken">A cancellation token provided by the
        /// queue that can be used to cancel message processing</param>
        /// <returns>Returns a task that completes when the message is processed</returns>
        async Task IQueueListener.MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken)
        {
            IEndpointCredentials credentials = null;
            if (_endpoints.TryGetEndpointByAddress(message.Headers.Destination, out IEndpoint endpoint))
            {
                credentials = endpoint.Credentials;
            }
            cancellationToken.ThrowIfCancellationRequested();
            Thread.CurrentPrincipal = context.Principal;
            await TransportMessage(message, credentials, cancellationToken);
            await context.Acknowledge();
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends a message directly to the application identified by the
        /// <see cref="P:Platibus.IMessageHeaders.Destination" /> header.
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
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message.Headers.Destination == null) throw new ArgumentException("Message has no destination");

            if (message.Headers.Synchronous)
            {
                await TransportMessage(message, credentials, cancellationToken);
                return;
            }

            await _messageQueueingService.EnqueueMessage(_outboundQueueName, message, Thread.CurrentPrincipal, cancellationToken);
        }

        /// <inheritdoc />
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
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Published, cancellationToken);
            }

            var subscribers = await _subscriptionTrackingService.GetSubscribers(topicName, cancellationToken);
            var transportTasks = new List<Task>();

            foreach (var subscriber in subscribers)
            {
                IEndpointCredentials subscriberCredentials = null;
                if (_endpoints.TryGetEndpointByAddress(subscriber, out var subscriberEndpoint))
                {
                    subscriberCredentials = subscriberEndpoint.Credentials;
                }

                var perEndpointHeaders = new MessageHeaders(message.Headers)
                {
                    MessageId = MessageId.Generate(),
                    Destination = subscriber
                };

                var addressedMessage = new Message(perEndpointHeaders, message.Content);
                if (addressedMessage.Headers.Synchronous)
                {
                    transportTasks.Add(TransportMessage(addressedMessage, subscriberCredentials, cancellationToken));
                    continue;
                }

                await _messageQueueingService.EnqueueMessage(_outboundQueueName, addressedMessage, null, cancellationToken);
            }

            await Task.WhenAll(transportTasks);
        }

        public async Task ReceiveMessage(Message message, IPrincipal principal,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var messageReceivedHandlers = MessageReceived;
            if (messageReceivedHandlers != null)
            {
                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Received, cancellationToken);
                }

                var args = new TransportMessageEventArgs(message, principal, cancellationToken);
                await messageReceivedHandlers(this, args);
            }
        }

        private async Task TransportMessage(Message message, IEndpointCredentials credentials, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpClient httpClient = null;
            Uri postUri = null;
            int? status = null;
            var delivered = false;
            var sw = Stopwatch.StartNew();
            var httpClientTimeout = TimeSpan.Zero;
            var requestInitiated = false;
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                if (_messageJournal != null)
                {
                    await _messageJournal.Append(message, MessageJournalCategory.Sent, cancellationToken);
                }

                var endpointBaseUri = message.Headers.Destination.WithTrailingSlash();
                if (_bypassTransportLocalDestination && endpointBaseUri == _baseUri)
                {
                    delivered = await HandleLocalDelivery(message, cancellationToken, endpointBaseUri);
                    return;
                }

                postUri = BuildPostMessageUri(out var relativeUri, message, endpointBaseUri);

                var httpContent = new StringContent(message.Content);
                WriteHttpContentHeaders(message, httpContent);

                httpClient = await _httpClientFactory.GetClient(endpointBaseUri, credentials, cancellationToken);
                httpClientTimeout = httpClient.Timeout;
                requestInitiated = true;
                httpResponseMessage = await httpClient.PostAsync(relativeUri, httpContent, cancellationToken);
                status = (int) httpResponseMessage.StatusCode;

                HandleHttpErrorResponse(httpResponseMessage);
                delivered = true;
            }
            catch (TransportException ex)
            {
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                    {
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status,
                        Exception = ex
                    }.Build());

                throw;
            }
            catch (UnauthorizedAccessException)
            {
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, DiagnosticEventType.AccessDenied)
                    {
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status
                    }.Build());
                throw;
            }
            catch (MessageNotAcknowledgedException)
            {
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                    {
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status
                    }.Build());
                throw;
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                var elapsed = sw.Elapsed;
                var cancellationRequested = cancellationToken.IsCancellationRequested;
                var timeoutKnown = httpClientTimeout > TimeSpan.Zero;
                var timeoutExceeded = requestInitiated && timeoutKnown && elapsed > httpClientTimeout;
                var probableTimeout = !cancellationRequested || timeoutExceeded;

                var detail = $"HTTP transport operation was canceled after {elapsed.TotalSeconds} second(s).  " +
                             $"Cancellation {(cancellationToken.IsCancellationRequested ? "was" : "was not")} requested by the caller.  ";

                if (timeoutKnown)
                {
                    detail += $"The HTTP client timeout is set to {httpClientTimeout.TotalSeconds} second(s).  ";
                }

                if (probableTimeout)
                {
                    detail += "Cancelation was likely due to a timeout on the HTTP client.  " +
                              "This may be caused by setting Synchronous to 'true' when sending a message.";
                }

                _diagnosticService.Emit(
                    new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                    {
                        Detail = detail,
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status
                    }.Build());

                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error sending message ID {message.Headers.MessageId}";
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                    {
                        Detail = errorMessage,
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status,
                        Exception = ex
                    }.Build());

                TryHandleCommunicationException(ex, message.Headers.Destination);
                throw new TransportException(errorMessage, ex);
            }
            finally
            {
                sw.Stop();
                var eventType = delivered
                    ? DiagnosticEventType.MessageDelivered
                    : DiagnosticEventType.MessageDeliveryFailed;
                
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, eventType)
                    {
                        Message = message,
                        Method = HttpMethod.Post.ToString(),
                        Uri = postUri,
                        Status = status
                    }.Build());

                httpResponseMessage?.Dispose();
                httpClient?.Dispose();
            }
        }

        
        /// <inheritdoc />
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
                        await SendSubscriptionRequest(subscription, cancellationToken);

                        if (!subscription.Expires)
                        {
                            // Subscription is not set to expire on the remote server.  Since
                            // publications are pushed to the subscribers, we don't need to keep
                            // this task running.
                            return;
                        }
                        
                        retryOrRenewAfter = subscription.RenewalInterval;

                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, DiagnosticEventType.SubscriptionRenewed)
                            {
                                Detail = "Subscription renewed.  Next renewal in " + retryOrRenewAfter,
                                Topic = topicName,
                            }.Build());
                    }
                    catch (EndpointNotFoundException enfe)
                    {
                        // Endpoint is not defined in the supplied configuration,
                        // so we cannot determine the URI.  This is an unrecoverable
                        // error, so simply return.
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, DiagnosticEventType.EndpointNotFound)
                            {
                                Detail = "Fatal error sending subscription request: endpoint not found",
                                Topic = topicName,
                                Exception = enfe
                            }.Build());

                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, DiagnosticEventType.SubscriptionFailed)
                            {
                                Topic = topicName
                            }.Build());

                        return;
                    }
                    catch (NameResolutionFailedException nrfe)
                    {
                        // The transport was unable to resolve the hostname in the
                        // endpoint URI.  This may or may not be a temporary error.
                        // In either case, retry after 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                            {
                                Detail = "Non-fatal error sending subscription request: unable to resolve address for hostname " + nrfe.Hostname + ".  Retry in " + retryOrRenewAfter,
                                Topic = topicName,
                                Exception = nrfe
                            }.Build());
                    }
                    catch (ConnectionRefusedException cre)
                    {
                        // The transport was unable to resolve the hostname in the
                        // endpoint URI.  This may or may not be a temporary error.
                        // In either case, retry after 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                            {
                                Detail = "Non-fatal error sending subscription request: connection refused for " + cre.Host + ":" + cre.Port + ".  Retry in " + retryOrRenewAfter,
                                Topic = topicName,
                                Exception = cre
                            }.Build());
                    }
                    catch (ResourceNotFoundException ire)
                    {
                        // Topic is not found.  This may be temporary.
                        retryOrRenewAfter = subscription.RetryInterval;
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                            {
                                Detail = "Non-fatal error sending subscription request: resource not found.  Topic may be misconfigured.  Retry in " + retryOrRenewAfter,
                                Topic = topicName,
                                Exception = ire
                            }.Build());
                    }
                    catch (InvalidRequestException ire)
                    {
                        // Request is not valid.  Either the URL is malformed or the
                        // topic does not exist.  In any case, retrying would be
                        // fruitless, so just return.
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, DiagnosticEventType.EndpointNotFound)
                            {
                                Detail = "Fatal error sending subscription request: invalid request.  Subscription abandoned.",
                                Topic = topicName,
                                Exception = ire
                            }.Build());

                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, DiagnosticEventType.SubscriptionFailed)
                            {
                                Topic = topicName
                            }.Build());

                        return;
                    }
                    catch (TransportException te)
                    {
                        // Unspecified transport error.  This may or may not be
                        // due to temporary conditions that will resolve 
                        // themselves.  Retry in 30 seconds.
                        retryOrRenewAfter = subscription.RetryInterval;
                        _diagnosticService.Emit(
                            new HttpEventBuilder(this, HttpEventType.HttpCommunicationError)
                            {
                                Detail = "Non-fatal error sending subscription request: communication error.  Retry in " + retryOrRenewAfter,
                                Topic = topicName,
                                Exception = te
                            }.Build());
                    }
                    await Task.Delay(retryOrRenewAfter, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static Uri BuildPostMessageUri(out string relativeUri, Message message, Uri endpointBaseUri)
        {
            var messageId = message.Headers.MessageId;
            var urlEncodedMessageId = UrlEncoder.Encode(messageId);
            relativeUri = $"message/{urlEncodedMessageId}";
            var postUri = new Uri(endpointBaseUri, relativeUri);
            return postUri;
        }

        private async Task<bool> HandleLocalDelivery(Message message, CancellationToken cancellationToken, Uri endpointBaseUri)
        {
            await _diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpTransportBypassed)
                {
                    Message = message,
                    Uri = endpointBaseUri
                }.Build(), cancellationToken);

            await ReceiveMessage(message, Thread.CurrentPrincipal, cancellationToken);

            return true;
        }


        private async Task SendSubscriptionRequest(HttpSubscriptionMetadata subscriptionMetadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (subscriptionMetadata == null) throw new ArgumentNullException(nameof(subscriptionMetadata));

            var endpoint = subscriptionMetadata.Endpoint;
            var topic = subscriptionMetadata.Topic;
            var ttl = subscriptionMetadata.TTL;
            
            HttpClient httpClient = null;
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                var endpointBaseUri = endpoint.Address.WithTrailingSlash();
                httpClient = await _httpClientFactory.GetClient(endpointBaseUri, endpoint.Credentials, cancellationToken);

                var urlSafeTopicName = UrlEncoder.Encode(topic);
                var relativeUri = $"topic/{urlSafeTopicName}/subscriber?uri={_baseUri}";
                if (ttl > TimeSpan.Zero)
                {
                    relativeUri += "&ttl=" + ttl.TotalSeconds;
                }

                var postUri = new Uri(endpointBaseUri, relativeUri);
                httpResponseMessage = await httpClient.PostAsync(relativeUri, new StringContent(""), cancellationToken);
                var status = (int?)httpResponseMessage.StatusCode;

                await _diagnosticService.EmitAsync(
                    new HttpEventBuilder(this, HttpEventType.HttpSubscriptionRequestSent)
                    {
                        Uri = postUri,
                        Topic = subscriptionMetadata.Topic,
                        Status = status
                    }.Build(), cancellationToken);

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
                var errorMessage =
                    $"Error sending subscription request for topic {topic} of publisher {endpoint.Address}";
                
                TryHandleCommunicationException(ex, endpoint.Address);

                throw new TransportException(errorMessage, ex);
            }
            finally
            {
                httpResponseMessage?.Dispose();
                httpClient?.Dispose();
            }
        }

        private static void HandleHttpErrorResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var statusCode = (int) response.StatusCode;
            var statusDescription = response.ReasonPhrase;

            if (statusCode == 401)
            {
                throw new UnauthorizedAccessException($"HTTP {statusCode}: {statusDescription}");
            }

            if (statusCode == 422)
            {
                throw new MessageNotAcknowledgedException($"HTTP {statusCode}: {statusDescription}");
            }

			if (statusCode == 404)
			{
				throw new ResourceNotFoundException($"HTTP {statusCode}: {statusDescription}");
			}

            if (statusCode < 500)
            {
                // Other HTTP 400-499 status codes identify invalid requests (bad request, authentication required, 
				// not authorized, etc.)
                throw new InvalidRequestException($"HTTP {statusCode}: {statusDescription}");
            }

            // HTTP 500+ are internal server errors
            throw new TransportException($"HTTP {statusCode}: {statusDescription}");
        }

        private static void TryHandleCommunicationException(Exception ex, Uri uri)
        {
            var handled = false;
            while (!handled)
            {
                switch (ex)
                {
                    case HttpRequestException hre when hre.InnerException != null:
                        ex = hre.InnerException;
                        continue;
                    case WebException we:
                        switch (we.Status)
                        {
                            case WebExceptionStatus.NameResolutionFailure:
                                throw new NameResolutionFailedException(uri.Host);
                            case WebExceptionStatus.ConnectFailure:
                                throw new ConnectionRefusedException(uri.Host, uri.Port, ex.InnerException ?? ex);
                        }
                        break;
                    case SocketException se:
                        switch (se.SocketErrorCode)
                        {
                            case SocketError.ConnectionRefused:
                                throw new ConnectionRefusedException(uri.Host, uri.Port, ex.InnerException ?? ex);
                            case SocketError.HostNotFound:
                                throw new NameResolutionFailedException(uri.Host);
                        }
                        break;
                    default:
                        // Because .NET Core applications have platform specific exceptions and
                        // error codes, some fuzzy logic is needed to determine whether a known
                        // category of transport exception has occurred.

                        var message = ex.Message.ToLower();
                        if (message.Contains("connection"))
                        {
                            throw new ConnectionRefusedException(uri.Host, uri.Port, ex.InnerException ?? ex);
                        }

                        var concernsHostname = message.Contains("host") || message.Contains("name");
                        var resolutionFailure = message.Contains("unknown") || message.Contains("resolve");
                        if (concernsHostname && resolutionFailure)
                        {
                            throw new NameResolutionFailedException(uri.Host);
                        }
                        break;

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
                if (_httpClientFactory is IDisposable disposableClientFactory)
                {
                    disposableClientFactory.Dispose();
                }
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