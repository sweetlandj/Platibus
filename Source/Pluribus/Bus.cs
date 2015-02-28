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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Pluribus.Config;
using Pluribus.Serialization;
using Pluribus.Security;

namespace Pluribus
{
    public class Bus : IBus, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Core);

        private bool _disposed;
        private readonly Uri _baseUri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IDictionary<EndpointName, IEndpoint> _endpoints;
        private readonly IList<IHandlingRule> _handlingRules;
        private readonly IMessageNamingService _messageNamingService;
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly QueueName _outboundQueueName;
        private readonly MemoryCacheReplyHub _replyHub = new MemoryCacheReplyHub(TimeSpan.FromMinutes(5));
        private readonly IList<ISendRule> _sendRules;
        private readonly ISerializationService _serializationService;
        private readonly IList<ISubscription> _subscriptions;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly IList<TopicName> _topics;
        private readonly ITransportService _transportService;

        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        public ITransportService TransportService
        {
            get { return _transportService; }
        }

        public IEnumerable<TopicName> Topics
        {
            get { return _topics; }
        }

        public Bus(IPluribusConfiguration configuration, ITransportService transportService)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (transportService == null) throw new ArgumentNullException("transportService");

            _baseUri = configuration.BaseUri;

            // TODO: Throw configuration exception if message queueing service, message naming
            // service, or serialization service are null

            _messageJournalingService = configuration.MessageJournalingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageNamingService = configuration.MessageNamingService;
            _serializationService = configuration.SerializationService;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;

            _endpoints = configuration.Endpoints.ToDictionary(d => d.Key, d => d.Value);
            _topics = configuration.Topics.ToList();
            _sendRules = configuration.SendRules.ToList();
            _handlingRules = configuration.HandlingRules.ToList();
            _subscriptions = configuration.Subscriptions.ToList();

            _transportService = transportService;
            _transportService.MessageReceived += OnMessageReceived;
            _transportService.SubscriptionRequestReceived += OnSubscriptionRequestReceived;

            _outboundQueueName = "Outbound";
        }

        public async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            var handlingRulesGroupedByQueueName = _handlingRules
                .GroupBy(r => r.QueueName)
                .ToDictionary(grp => grp.Key, grp => grp);

            foreach (var ruleGroup in handlingRulesGroupedByQueueName)
            {
                var queueName = ruleGroup.Key;
                var rules = ruleGroup.Value;
                var handlers = rules.Select(r => r.MessageHandler);

                await _messageQueueingService
                    .CreateQueue(queueName, new MessageHandlingListener(this, _messageNamingService, _serializationService, handlers))
                    .ConfigureAwait(false);
            }

            await _messageQueueingService
                .CreateQueue(_outboundQueueName, new OutboundQueueListener(_transportService, _messageJournalingService))
                .ConfigureAwait(false);

            foreach (var subscription in _subscriptions)
            {
                // Do not await these; it uses a loop with Task.Delay to ensure that the subscriptions
                // are periodically renewed.

                // ReSharper disable once UnusedVariable
                var subscriptionTask = Subscribe(subscription, _cancellationTokenSource.Token);
            }
        }
        
        public async Task<ISentMessage> Send(object content, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");

            var prototypicalMessage = BuildMessage(content, options: options);
            var endpoints = GetEndpointsForMessage(prototypicalMessage);
            var transportTasks = new List<Task>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var kvp in endpoints)
            {
                var endpointName = kvp.Key;
                var endpoint = kvp.Value;
                var perEndpointHeaders = new MessageHeaders(prototypicalMessage.Headers)
                {
                    Destination = endpoint.Address
                };

                Log.DebugFormat("Sending message ID {0} to endpoint \"{1}\" ({2})...",
                    prototypicalMessage.Headers.MessageId, endpointName, endpoint.Address);

                var addressedMessage = new Message(perEndpointHeaders, prototypicalMessage.Content);
                transportTasks.Add(TransportMessage(addressedMessage, options, cancellationToken));
            }
            await Task.WhenAll(transportTasks).ConfigureAwait(false);
            return _replyHub.CreateSentMessage(prototypicalMessage);
        }

        public async Task<ISentMessage> Send(object content, EndpointName endpointName,
            SendOptions options = default(SendOptions), CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");
            if (endpointName == null) throw new ArgumentNullException("endpointName");
            
            var endpoint = _endpoints[endpointName];
            var headers = new MessageHeaders
            {
                Destination = endpoint.Address
            };
            var message = BuildMessage(content, headers, options);

            Log.DebugFormat("Sending message ID {0} to endpoint \"{1}\" ({2})...", 
                message.Headers.MessageId, endpointName, endpoint.Address);

            await TransportMessage(message, options, cancellationToken).ConfigureAwait(false);
            return _replyHub.CreateSentMessage(message);
        }

        public async Task<ISentMessage> Send(object content, Uri endpointUri, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");
            if (endpointUri == null) throw new ArgumentNullException("endpointUri");

            var headers = new MessageHeaders
            {
                Destination = endpointUri
            };
            var message = BuildMessage(content, headers, options);

            Log.DebugFormat("Sending message ID {0} to \"{2}\"...",
                message.Headers.MessageId, endpointUri);
            
            await TransportMessage(message, options, cancellationToken).ConfigureAwait(false);
            return _replyHub.CreateSentMessage(message);
        }

        public async Task Publish(object content, TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (!_topics.Contains(topic)) throw new TopicNotFoundException(topic);

            var prototypicalHeaders = new MessageHeaders
            {
                Topic = topic,
                Published = DateTime.UtcNow
            };

            var options = new SendOptions 
            {
                UseDurableTransport = true
            };

            var prototypicalMessage = BuildMessage(content, prototypicalHeaders, options);
            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessagePublished(prototypicalMessage).ConfigureAwait(false);
            }

            var subscribers = _subscriptionTrackingService.GetSubscribers(topic);

            var publishSendOptions = new SendOptions();
            var transportTasks = new List<Task>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var subscriber in subscribers)
            {
                var perEndpointHeaders = new MessageHeaders(prototypicalMessage.Headers)
                {
                    Destination = subscriber
                };

                var addressedMessage = new Message(perEndpointHeaders, prototypicalMessage.Content);
                transportTasks.Add(TransportMessage(addressedMessage, publishSendOptions, cancellationToken));
            }

            await Task.WhenAll(transportTasks).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IEndpoint GetEndpoint(EndpointName endpointName)
        {
            IEndpoint endpoint;
            if (!_endpoints.TryGetValue(endpointName, out endpoint))
            {
                throw new EndpointNotFoundException(endpointName);
            }
            return endpoint;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task Subscribe(ISubscription subscription,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Uri publisher;
                try
                {
                    publisher = GetEndpoint(subscription.Publisher).Address;
                }
                catch (EndpointNotFoundException enfe)
                {
                    // Endpoint is not defined in the supplied configuration,
                    // so we cannot determine the URI.  This is an unrecoverable
                    // error, so simply return.
                    Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint \"{1}\"", enfe, subscription.Topic,
                        subscription.Publisher);
                    return;
                }

                TimeSpan retryOrRenewAfter;
                try
                {
                    Log.DebugFormat("Sending subscription request for topic {0} to {1}...", subscription.Topic,
                        publisher);

                    await _transportService.SendSubscriptionRequest(SubscriptionRequestType.Add, publisher,
                        subscription.Topic, _baseUri, subscription.TTL, cancellationToken).ConfigureAwait(false);

                    if (subscription.TTL <= TimeSpan.Zero)
                    {
                        // Subscription does not expire, so no need to schedule a renewal.
                        Log.DebugFormat(
                            "Subscription request for topic {0} successfuly sent to {1}.  Subscription has no configured TTL and is not set to auto-renew.",
                            subscription.Topic, publisher);
                        return;
                    }

                    // Attempt to renew after half of the TTL to allow for
                    // issues that may occur when attempting to renew the
                    // subscription.
                    retryOrRenewAfter = TimeSpan.FromMilliseconds(subscription.TTL.TotalMilliseconds/2);
                    Log.DebugFormat(
                        "Subscription request for topic {0} successfuly sent to {1}.  Subscription TTL is {2} and is scheduled to auto-renew in {3}",
                        subscription.Topic, publisher, subscription.TTL, retryOrRenewAfter);
                }
                catch (EndpointNotFoundException enfe)
                {
                    // Endpoint is not defined in the supplied configuration,
                    // so we cannot determine the URI.  This is an unrecoverable
                    // error, so simply return.
                    Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint \"{1}\"", enfe, subscription.Topic,
                        publisher);
                    return;
                }
                catch (NameResolutionFailedException nrfe)
                {
                    // The transport was unable to resolve the hostname in the
                    // endpoint URI.  This may or may not be a temporary error.
                    // In either case, retry after 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", nrfe,
                        subscription.Topic, publisher, retryOrRenewAfter);
                }
                catch (ConnectionRefusedException cre)
                {
                    // The transport was unable to resolve the hostname in the
                    // endpoint URI.  This may or may not be a temporary error.
                    // In either case, retry after 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", cre,
                        subscription.Topic, publisher, retryOrRenewAfter);
                }
                catch (InvalidRequestException ire)
                {
                    // Request is not valid.  Either the URL is malformed or the
                    // topic does not exist.  In any case, retrying would be
                    // fruitless, so just return.
                    Log.ErrorFormat("Fatal error subscribing to topic {0} of endpoint {1}", ire, subscription.Topic,
                        publisher);
                    return;
                }
                catch (TransportException te)
                {
                    // Unspecified transport error.  This may or may not be
                    // due to temporary conditions that will resolve 
                    // themselves.  Retry in 30 seconds.
                    retryOrRenewAfter = TimeSpan.FromSeconds(30);
                    Log.WarnFormat("Non-fatal error subscribing to topic {0} of endpoint {1}.  Retrying in {2}", te,
                        subscription.Topic, publisher, retryOrRenewAfter);
                }

                await Task.Delay(retryOrRenewAfter, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private Message BuildMessage(object content, IMessageHeaders suppliedHeaders = null, SendOptions options = default(SendOptions))
        {
            if (content == null) throw new ArgumentNullException("content");
            var messageName = _messageNamingService.GetNameForType(content.GetType());
            var headers = new MessageHeaders(suppliedHeaders)
            {
                MessageId = MessageId.Generate(),
                MessageName = messageName,
                Origination = _baseUri
            };

            var contentType = options.ContentType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = "application/json";
            }
            headers.ContentType = contentType;

            var serializer = _serializationService.GetSerializer(headers.ContentType);
            var serializedContent = serializer.Serialize(content);
            return new Message(headers, serializedContent);
        }

        private IEnumerable<KeyValuePair<EndpointName, IEndpoint>> GetEndpointsForMessage(Message message)
        {
            return _sendRules
                .Where(r => r.Specification.IsSatisfiedBy(message))
                .SelectMany(r => r.Endpoints)
                .Join(_endpoints, n => n, d => d.Key, (n, d) => new {Name = n, Endpoint = d.Value})
                .ToDictionary(x => x.Name, x => x.Endpoint);
        }

        internal async Task SendReply(BusMessageContext messageContext, object replyContent,
            SendOptions options = default(SendOptions), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (messageContext == null) throw new ArgumentNullException("messageContext");
            if (replyContent == null) throw new ArgumentNullException("replyContent");

            var headers = new MessageHeaders
            {
                Destination = messageContext.Headers.Origination,
                RelatedTo = messageContext.Headers.MessageId
            };
            var replyMessage = BuildMessage(replyContent,headers, options);
            await TransportMessage(replyMessage, options, cancellationToken).ConfigureAwait(false);
        }

        private async Task TransportMessage(Message message, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (options.UseDurableTransport)
            {
                var senderPrincipal = Thread.CurrentPrincipal;
                Log.DebugFormat("Durable transport requested.  Enqueueing message ID {0} in outbound queue \"{1}\"...", message.Headers.MessageId, _outboundQueueName);
                await _messageQueueingService
                    .EnqueueMessage(_outboundQueueName, message, senderPrincipal)
                    .ConfigureAwait(false);
                Log.DebugFormat("Message ID {0} enqueued successfully", message.Headers.MessageId);
            }
            else
            {
                Log.DebugFormat("Durable transport not requested.  Attempting to transport message ID {0} directly to destination \"{1}\"...", message.Headers.MessageId, message.Headers.Destination);
                await _transportService.SendMessage(message, cancellationToken).ConfigureAwait(false);
                Log.DebugFormat("Message ID {0} transported successfully", message.Headers.MessageId);

                if (_messageJournalingService != null)
                {
                    await _messageJournalingService.MessageSent(message).ConfigureAwait(false);
                }
            }
        }

        private async void OnMessageReceived(object source, MessageReceivedEventArgs args)
        {
            var message = args.Message;

            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessageReceived(message);
            }

            var matchingRules = _handlingRules
                .Where(r => r.MessageSpecification.IsSatisfiedBy(message))
                .ToList();

            // Make sure that the principal is serializable before enqueuing
            var senderPrincipal = args.Principal as SenderPrincipal;
            if (senderPrincipal == null && args.Principal != null)
            {
                senderPrincipal = new SenderPrincipal(args.Principal);
            }

            // Message expiration handled in MessageHandlingListener
            var tasks = matchingRules
                .Select(rule => rule.QueueName)
                .Distinct()
                .Select(q => _messageQueueingService.EnqueueMessage(q, message, senderPrincipal))
                .ToList();

            var relatedToMessageId = message.Headers.RelatedTo;
            if (relatedToMessageId != default(MessageId))
            {
                tasks.Add(NotifyReplyReceived(message));
            }
            
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task NotifyReplyReceived(Message message)
        {
            // TODO: Handle special "last reply" message.  Presently we only support a single reply.
            // This will probably be a special function of the ITransportService and will likely
            // be, for example, an empty POST to {baseUri}/message/{messageId}?lastReply=true, which
            // will trigger the OnComplete event in the IObservable reply stream.  However, we need
            // to put some thought into potential issues around the sequence and timing of replies
            // vs. the final lastReply POST to ensure that all replies are processed before the
            // OnComplete event is triggered.  One possibility is a reply sequence number header
            // and an indication of the total number of replies in the lastReply POST.  If the
            // number of replies received is less than the number expected, then the OnComplete
            // event can be deferred.

            var relatedToMessageId = message.Headers.RelatedTo;
            var messageType = _messageNamingService.GetTypeForName(message.Headers.MessageName);
            var serializer = _serializationService.GetSerializer(message.Headers.ContentType);
            var messageContent = serializer.Deserialize(message.Content, messageType);

            await _replyHub.ReplyReceived(messageContent, relatedToMessageId);
            await _replyHub.NotifyLastReplyReceived(relatedToMessageId);
        }

        private async void OnSubscriptionRequestReceived(object source, SubscriptionRequestReceivedEventArgs args)
        {
            var topic = args.Topic;
            var subscriber = args.Subscriber;
            var ttl = args.TTL;
            await _subscriptionTrackingService.AddSubscription(topic, subscriber, ttl).ConfigureAwait(false);
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~Bus()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _cancellationTokenSource.Cancel();
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                _replyHub.Dispose();
            }
            _disposed = true;
        }

        private class OutboundQueueListener : IQueueListener
        {
            private readonly ITransportService _transportService;
            private readonly IMessageJournalingService _messageJournalingService;

            public OutboundQueueListener(ITransportService transportService, IMessageJournalingService messageJournalingService)
            {
                if (transportService == null) throw new ArgumentNullException("transportService");
                _transportService = transportService;
                _messageJournalingService = messageJournalingService;
            }

            public async Task MessageReceived(Message message, IQueuedMessageContext context,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                if (message.Headers.Expires < DateTime.UtcNow)
                {
                    Log.WarnFormat("Discarding expired \"{0}\" message (ID {1}, expired {2})",
                        message.Headers.MessageName, message.Headers.MessageId, message.Headers.Expires);
                    return;
                }

                await _transportService.SendMessage(message, cancellationToken).ConfigureAwait(false);
                await context.Acknowledge().ConfigureAwait(false);

                if (_messageJournalingService != null)
                {
                    await _messageJournalingService.MessageSent(message).ConfigureAwait(false);
                }
            }
        }
    }
}