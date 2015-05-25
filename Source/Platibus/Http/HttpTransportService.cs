// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;

namespace Platibus.Http
{
    public class HttpTransportService : ITransportService
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);

        private readonly Uri _baseUri;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;

        public HttpTransportService(Uri baseUri, ISubscriptionTrackingService subscriptionTrackingService)
        {
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (subscriptionTrackingService == null) throw new ArgumentNullException("subscriptionTrackingService");
            _baseUri = baseUri;
            _subscriptionTrackingService = subscriptionTrackingService;
        }

        private static HttpClient GetClient(Uri uri, IEndpointCredentials credentials)
        {
            var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseProxy = false
            };

            if (credentials != null)
            {
                credentials.Accept(new HttpEndpointCredentialsVisitor(clientHandler));
            }

            var httpClient = new HttpClient(clientHandler)
            {
                BaseAddress = uri
            };

            return httpClient;
        }

        public async Task SendMessage(Message message, IEndpointCredentials credentials = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Headers.Destination == null) throw new ArgumentException("Message has no destination");
            try
            {
                var httpContent = new StringContent(message.Content);
                WriteHttpContentHeaders(message, httpContent);
                var endpointBaseUri = message.Headers.Destination;

                var httpClient = GetClient(endpointBaseUri, credentials);

                var messageId = message.Headers.MessageId;
                var urlEncondedMessageId = HttpUtility.UrlEncode(messageId);
                var relativeUri = string.Format("message/{0}", urlEncondedMessageId);
                var httpResponseMessage = await httpClient.PostAsync(relativeUri, httpContent, cancellationToken);

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
        }

        public async Task PublishMessage(Message message, TopicName topicName, CancellationToken cancellationToken)
        {
            var subscribers = await _subscriptionTrackingService.GetSubscribers(topicName, cancellationToken);
            var transportTasks = new List<Task>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var subscriber in subscribers)
            {
                var perEndpointHeaders = new MessageHeaders(message.Headers)
                {
                    Destination = subscriber
                };

                var addressedMessage = new Message(perEndpointHeaders, message.Content);
                transportTasks.Add(SendMessage(addressedMessage, null, cancellationToken));
            }

            await Task.WhenAll(transportTasks);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        public async Task Subscribe(Uri publisherUri, TopicName topicName, TimeSpan ttl = default(TimeSpan),
            IEndpointCredentials credentials = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan retryOrRenewAfter;
                try
                {
                    Log.DebugFormat("Sending subscription request for topic {0} to {1}...", topicName, publisherUri);

                    await SendSubscriptionRequest(publisherUri, credentials, topicName, TimeSpan.FromHours(1), cancellationToken);

                    if (ttl <= TimeSpan.Zero)
                    {
                        // No TTL specified; subscription lives forever on the
                        // remote server.  Since publications are pushed to the
                        // subscribers, we don't need to keep this task running.
                        return;
                    }

                    // Attempt to renew after half of the TTL to allow for
                    // issues that may occur when attempting to renew the
                    // subscription.
                    retryOrRenewAfter = TimeSpan.FromTicks(ttl.Ticks/2);
                    Log.DebugFormat(
                        "Subscription request for topic {0} successfuly sent to {1}.  Subscription TTL is {2} and is scheduled to auto-renew in {3}",
                        topicName, publisherUri, ttl, retryOrRenewAfter);
                }
                catch (EndpointNotFoundException enfe)
                {
                    // Endpoint is not defined in the supplied configuration,
                    // so we cannot determine the URI.  This is an unrecoverable
                    // error, so simply return.
                    Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint \"{1}\"", enfe, topicName,
                        publisherUri);
                    return;
                }
                catch (NameResolutionFailedException nrfe)
                {
                    // The transport was unable to resolve the hostname in the
                    // endpoint URI.  This may or may not be a temporary error.
                    // In either case, retry after 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", nrfe,
                        topicName, publisherUri, retryOrRenewAfter);
                }
                catch (ConnectionRefusedException cre)
                {
                    // The transport was unable to resolve the hostname in the
                    // endpoint URI.  This may or may not be a temporary error.
                    // In either case, retry after 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", cre,
                        topicName, publisherUri, retryOrRenewAfter);
                }
                catch (InvalidRequestException ire)
                {
                    // Request is not valid.  Either the URL is malformed or the
                    // topic does not exist.  In any case, retrying would be
                    // fruitless, so just return.
                    Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint {1}", ire, topicName,
                        publisherUri);
                    return;
                }
                catch (TransportException te)
                {
                    // Unspecified transport error.  This may or may not be
                    // due to temporary conditions that will resolve 
                    // themselves.  Retry in 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", te,
                        topicName, publisherUri, retryOrRenewAfter);
                }

                await Task.Delay(retryOrRenewAfter, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task SendSubscriptionRequest(Uri publisherUri, IEndpointCredentials credentials, TopicName topic,
            TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (publisherUri == null) throw new ArgumentNullException("publisherUri");
            if (topic == null) throw new ArgumentNullException("topic");
            
            try
            {
                var httpClient = GetClient(publisherUri, credentials);

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
                    topic, publisherUri);
                Log.ErrorFormat(errorMessage, ex);

                HandleCommunicationException(ex, publisherUri);

                throw new TransportException(errorMessage, ex);
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

            if (statusCode < 500)
            {
                // HTTP 400-499 are invalid requests (bad request, authentication required, not authorized, etc.)
                throw new InvalidRequestException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
            }

            // HTTP 500+ are internal server errors
            throw new TransportException(string.Format("HTTP {0}: {1}", statusCode, statusDescription));
        }

        private static void HandleCommunicationException(Exception ex, Uri uri)
        {
            var hre = ex as HttpRequestException;
            if (hre != null && hre.InnerException != null)
            {
                HandleCommunicationException(hre.InnerException, uri);
                return;
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
    }
}