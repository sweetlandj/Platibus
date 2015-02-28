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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;

namespace Pluribus.Http
{
    public class HttpTransportService : ITransportService
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);
        public event MessageReceivedHandler MessageReceived;
        public event SubscriptionRequestReceivedHandler SubscriptionRequestReceived;

        public async Task SendMessage(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Headers.Destination == null) throw new ArgumentException("Message has no destination");
            try
            {
                var httpContent = new StringContent(message.Content);
                WriteHttpContentHeaders(message, httpContent);
                var endpointBaseUri = message.Headers.Destination;
                var httpClientHandler = new HttpClientHandler 
                { 
                    AllowAutoRedirect = true,
                    UseProxy = false,
                    UseDefaultCredentials = true
                };

                var httpClient = new HttpClient(httpClientHandler)
                {
                    BaseAddress = endpointBaseUri
                };

                var messageId = message.Headers.MessageId;
                var urlEncondedMessageId = HttpUtility.UrlEncode(messageId);
                var relativeUri = string.Format("message/{0}", urlEncondedMessageId);
                var httpResponseMessage = await httpClient
                    .PostAsync(relativeUri, httpContent, cancellationToken)
                    .ConfigureAwait(false);

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
                var errorMessage = string.Format("Error sending message ID {0}", message.Headers.MessageId);
                Log.ErrorFormat(errorMessage, ex);

                HandleCommunicationException(ex, message.Headers.Destination);

                throw new TransportException(errorMessage, ex);
            }
        }

        public async Task SendSubscriptionRequest(SubscriptionRequestType requestType, Uri publisher, TopicName topic,
            Uri subscriber, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (topic == null) throw new ArgumentNullException("topic");
            if (subscriber == null) throw new ArgumentNullException("subscriber");

            try
            {
                var clientHandler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    UseProxy = false,
                    UseDefaultCredentials = true
                };

                var httpClient = new HttpClient(clientHandler)
                {
                    BaseAddress = publisher
                };

                var urlSafeTopicName = HttpUtility.UrlEncode(topic);
                var relativeUri = string.Format("topic/{0}/subscriber?uri={1}", urlSafeTopicName, subscriber);
                if (ttl > TimeSpan.Zero)
                {
                    relativeUri += "&ttl=" + ttl.TotalSeconds;
                }

                HttpResponseMessage httpResponseMessage;
                switch (requestType)
                {
                    case SubscriptionRequestType.Remove:
                        httpResponseMessage = await httpClient
                            .DeleteAsync(relativeUri, cancellationToken)
                            .ConfigureAwait(false);
                        break;
                    default:
                        httpResponseMessage = await httpClient
                            .PostAsync(relativeUri, new StringContent(""), cancellationToken)
                            .ConfigureAwait(false);
                        break;
                }

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
                var errorMessage = string.Format("Error sending subscription request for topic {0} of publisher {1}", topic, publisher);
                Log.ErrorFormat(errorMessage, ex);

                HandleCommunicationException(ex, publisher);

                throw new TransportException(errorMessage, ex);
            }
        }

        public Task AcceptMessage(Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var handlers = MessageReceived;
                if (handlers != null)
                {
                    handlers(this, new MessageReceivedEventArgs(message, senderPrincipal));
                }
            }, cancellationToken);
        }

        public Task AcceptSubscriptionRequest(SubscriptionRequestType requestType, TopicName topic, Uri subscriber, TimeSpan ttl, IPrincipal senderPrincipal, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var handlers = SubscriptionRequestReceived;
                if (handlers != null)
                {
                    handlers(this, new SubscriptionRequestReceivedEventArgs(requestType, topic, subscriber, ttl, senderPrincipal));
                }
            }, cancellationToken);
        }

        private static void HandleHttpErrorResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var statusCode = (int) response.StatusCode;
            var statusDescription = response.ReasonPhrase;
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