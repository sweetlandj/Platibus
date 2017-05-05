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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Platibus.Config;
using Platibus.Serialization;

namespace Platibus
{
    /// <summary>
    /// Default <see cref="IBus"/> implementation
    /// </summary>
    public class Bus : IBus, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Core);

        private bool _disposed;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IEndpointCollection _endpoints;
        private readonly IList<Task> _subscriptionTasks = new List<Task>();
        private readonly IList<IHandlingRule> _handlingRules;
        private readonly IMessageNamingService _messageNamingService;
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly IMessageQueueingService _messageQueueingService;
	    private readonly string _defaultContentType;
        
        private readonly MemoryCacheReplyHub _replyHub = new MemoryCacheReplyHub(TimeSpan.FromMinutes(5));
        private readonly IList<ISendRule> _sendRules;
        private readonly ISerializationService _serializationService;
        private readonly IList<ISubscription> _subscriptions;
        private readonly IList<TopicName> _topics;
        private readonly Uri _baseUri;
        private readonly ITransportService _transportService;

        /// <summary>
        /// Initializes a new <see cref="Bus"/> with the specified configuration and services
        /// provided by the host
        /// </summary>
        /// <param name="configuration">The core bus configuration</param>
        /// <param name="baseUri">The base URI provided by the host</param>
        /// <param name="transportService">The transport service provided by the host</param>
        /// <param name="messageQueueingService">The message queueing service provided by the host</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are <c>null</c></exception>
        public Bus(IPlatibusConfiguration configuration, Uri baseUri, ITransportService transportService, IMessageQueueingService messageQueueingService)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (baseUri == null) throw new ArgumentNullException("baseUri");
            if (transportService == null) throw new ArgumentNullException("transportService");
            if (messageQueueingService == null) throw new ArgumentNullException("messageQueueingService");

            // Validate the provided configuration and throw exceptions for missing or invalid
            // configurations
            configuration.Validate();

            _baseUri = baseUri;
            _transportService = transportService;
            _messageQueueingService = messageQueueingService;
	        _defaultContentType = string.IsNullOrWhiteSpace(configuration.DefaultContentType)
		        ? "application/json"
		        : configuration.DefaultContentType;

            _messageJournalingService = configuration.MessageJournalingService;
            _messageNamingService = configuration.MessageNamingService;
            _serializationService = configuration.SerializationService;

            _endpoints = new ReadOnlyEndpointCollection(configuration.Endpoints);
            _topics = configuration.Topics.ToList();
            _sendRules = configuration.SendRules.ToList();
            _handlingRules = configuration.HandlingRules.ToList();
            _subscriptions = configuration.Subscriptions.ToList();
        }

        /// <summary>
        /// Initializes the bus instance
        /// </summary>
        /// <param name="cancellationToken">(Optional) A cancellation token provided by the
        /// caller that can be used to indicate that initialization should be canceled</param>
        /// <returns>Returns a task that will complete when bus initialization is complete</returns>
        /// <remarks>
        /// During initialization all handler queues are initialized and listeners are started.
        /// Additionally, subscriptions are initiated through the <see cref="ITransportService"/>.
        /// </remarks>
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

                var queueOptions = rules
                    .OrderBy(r => QueueOptionPrecedence(r.QueueOptions))
                    .Select(r => r.QueueOptions)
                    .FirstOrDefault();

                var queueListener = new MessageHandlingListener(this, _messageNamingService, _serializationService, handlers);
                await _messageQueueingService.CreateQueue(queueName, queueListener, queueOptions, cancellationToken);
            }

            foreach (var subscription in _subscriptions)
            {
                var endpoint = _endpoints[subscription.Endpoint];

                // The returned task will no complete until the subscription is
                // canceled via the supplied cancelation token, so we shouldn't
                // await it.

                var subscriptionTask = _transportService.Subscribe(endpoint, subscription.Topic, subscription.TTL,
                    _cancellationTokenSource.Token);

                _subscriptionTasks.Add(subscriptionTask);
            }
        }

        private static int QueueOptionPrecedence(QueueOptions queueOptions)
        {
            // Prefer overrides to default options
            return default(QueueOptions).Equals(queueOptions) ? 1 : 0;
        }

        private async Task TransportMessage(Message message, IEndpointCredentials credentials,
            CancellationToken cancellationToken)
        {
            await _transportService.SendMessage(message, credentials, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ISentMessage> Send(object content, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");

            var prototypicalMessage = BuildMessage(content, options: options);
            var endpoints = GetEndpointsForSend(prototypicalMessage);

            var transportTasks = new List<Task>();

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(prototypicalMessage);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var kvp in endpoints)
            {
                var endpointName = kvp.Key;
                var endpoint = kvp.Value;
                var credentials = endpoint.Credentials;
                if (options != null && options.Credentials != null)
                {
                    credentials = options.Credentials;
                }

                var perEndpointHeaders = new MessageHeaders(prototypicalMessage.Headers)
                {
                    Destination = endpoint.Address
                };

                Log.DebugFormat("Sending message ID {0} to endpoint \"{1}\" ({2})...",
                    prototypicalMessage.Headers.MessageId, endpointName, endpoint.Address);

                var addressedMessage = new Message(perEndpointHeaders, prototypicalMessage.Content);
                transportTasks.Add(TransportMessage(addressedMessage, credentials, cancellationToken));
            }
            await Task.WhenAll(transportTasks);
            return sentMessage;
        }
        
        /// <inheritdoc />
        public async Task<ISentMessage> Send(object content, EndpointName endpointName,
            SendOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");
            if (endpointName == null) throw new ArgumentNullException("endpointName");

            var endpoint = _endpoints[endpointName];
            var credentials = endpoint.Credentials;
            if (options != null && options.Credentials != null)
            {
                credentials = options.Credentials;
            }

            var headers = new MessageHeaders
            {
                Destination = endpoint.Address
            };
            var message = BuildMessage(content, headers, options);

            Log.DebugFormat("Sending message ID {0} to endpoint \"{1}\" ({2})...",
                message.Headers.MessageId, endpointName, endpoint.Address);

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(message);
            await TransportMessage(message, credentials, cancellationToken);
            return sentMessage;
        }

        /// <inheritdoc />
        [Obsolete("Endpoint credentials override has been moved to SendOptions")]
        public Task<ISentMessage> Send(object content, Uri endpointAddress, IEndpointCredentials credentials,
            SendOptions options = null, CancellationToken cancellationToken = new CancellationToken())
        {
            if (options != null && options.Credentials == null)
            {
                options.Credentials = credentials;
            }
            return Send(content, endpointAddress, options, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<ISentMessage> Send(object content, Uri endpointAddress,
            SendOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException("content");
            if (endpointAddress == null) throw new ArgumentNullException("endpointAddress");

            var headers = new MessageHeaders
            {
                Destination = endpointAddress
            };

            var message = BuildMessage(content, headers, options);
            var credentials = options == null ? null : options.Credentials;

            IEndpoint knownEndpoint;
            if (credentials == null && _endpoints.TryGetEndpointByAddress(endpointAddress, out knownEndpoint))
            {
                credentials = knownEndpoint.Credentials;
            }

            Log.DebugFormat("Sending message ID {0} to \"{2}\"...",
                message.Headers.MessageId, endpointAddress);

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(message);
            await TransportMessage(message, credentials, cancellationToken);
            return sentMessage;
        }
        
        /// <inheritdoc />
        public async Task Publish(object content, TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            if (!_topics.Contains(topic)) throw new TopicNotFoundException(topic);

            var prototypicalHeaders = new MessageHeaders
            {
                Topic = topic,
                Published = DateTime.UtcNow,
                
                Importance = MessageImportance.Critical
            };

            // All publications are set to critical importance to ensure
            // that they are queued on the sending side rather than
            // waiting for all subscribers to receive the messages
            // successfully
            var sendOptions = new SendOptions
            {
                Importance = MessageImportance.Critical
            };

            var message = BuildMessage(content, prototypicalHeaders, sendOptions);
            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessagePublished(message, cancellationToken);
            }

            await _transportService.PublishMessage(message, topic, cancellationToken);
        }

        private Message BuildMessage(object content, IMessageHeaders suppliedHeaders = null,
            SendOptions options = null)
        {
            if (content == null) throw new ArgumentNullException("content");
            var messageName = _messageNamingService.GetNameForType(content.GetType());
            var headers = new MessageHeaders(suppliedHeaders)
            {
                MessageId = MessageId.Generate(),
                MessageName = messageName,
                Origination = _baseUri,
                Importance = options == null ? default(MessageImportance) : options.Importance
            };

            var contentType = options == null ? null : options.ContentType;
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = _defaultContentType;
            }
            headers.ContentType = contentType;

            var serializer = _serializationService.GetSerializer(headers.ContentType);
            var serializedContent = serializer.Serialize(content);
            return new Message(headers, serializedContent);
        }

        private IEnumerable<KeyValuePair<EndpointName, IEndpoint>> GetEndpointsForSend(Message message)
        {
            var matchingSendRules = _sendRules
                .Where(rule => rule.Specification.IsSatisfiedBy(message))
                .ToList();

            if (!matchingSendRules.Any())
            {
                throw new NoMatchingSendRulesException();
            }

            var endpointNames = matchingSendRules
                .SelectMany(rule => rule.Endpoints)
                .ToList();

            var messageEndpoints = new Dictionary<EndpointName, IEndpoint>();
            foreach (var endpointName in endpointNames)
            {
                // Throws EndpointNotFoundException if no endpoint with the specified
                // name exists
                messageEndpoints[endpointName] = _endpoints[endpointName];
            }
            
            return messageEndpoints;
        }

        internal async Task SendReply(BusMessageContext messageContext, object replyContent,
            SendOptions options = default(SendOptions), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (messageContext == null) throw new ArgumentNullException("messageContext");
            if (replyContent == null) throw new ArgumentNullException("replyContent");

            IEndpoint replyToEndpoint;
            IEndpointCredentials credentials = null;
            var replyTo = messageContext.Headers.ReplyTo ?? messageContext.Headers.Origination;
            if (_endpoints.TryGetEndpointByAddress(replyTo, out replyToEndpoint))
            {
                credentials = replyToEndpoint.Credentials;
            }

            var headers = new MessageHeaders
            {
                Destination = messageContext.Headers.Origination,
                RelatedTo = messageContext.Headers.MessageId
            };
            var replyMessage = BuildMessage(replyContent, headers, options);
            await TransportMessage(replyMessage, credentials, cancellationToken);
        }

        /// <summary>
        /// Called by the host when a new message arrives to handle the message
        /// </summary>
        /// <param name="message">The new message</param>
        /// <param name="principal">The sender principal</param>
        /// <returns>Returns a task that completes when message handling is complete</returns>
        public async Task HandleMessage(Message message, IPrincipal principal)
        {
            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessageReceived(message);
            }

            var tasks = new List<Task>();
            var isPublication = message.Headers.Topic != null;
            var isReply = message.Headers.RelatedTo != default(MessageId);
            if (isReply)
            {
                tasks.Add(NotifyReplyReceived(message));
            }

            var importance = message.Headers.Importance;
            var queueMessage = isPublication || isReply || importance.RequiresQueueing;
            if (queueMessage)
            {
                // Message expiration handled in MessageHandlingListener
                tasks.AddRange(_handlingRules
                    .Where(r => r.Specification.IsSatisfiedBy(message))
                    .Select(rule => rule.QueueName)
                    .Distinct()
                    .Select(q => _messageQueueingService.EnqueueMessage(q, message, principal)));
            }
            else
            {
                tasks.Add(HandleMessageImmediately(message, principal));
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleMessageImmediately(Message message, IPrincipal senderPrincipal)
        {
            var messageContext = new BusMessageContext(this, message.Headers, senderPrincipal);
            var handlers = _handlingRules
                .Where(r => r.Specification.IsSatisfiedBy(message))
                .Select(rule => rule.MessageHandler)
                .ToList();
            
            await MessageHandler.HandleMessage(_messageNamingService, _serializationService,
                handlers, message, messageContext, _cancellationTokenSource.Token);
            
            if (!messageContext.MessageAcknowledged)
            {
                throw new MessageNotAcknowledgedException();
            }
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

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure that all resources are disposed
        /// </summary>
        ~Bus()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of managed and unmanaged resources
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from the
        /// <see cref="Dispose()"/> method; <c>false</c> if called from the
        /// finalizer</param>
        /// <remarks>
        /// Unmanaged resources should be disposed regardless of the value
        /// of the <paramref name="disposing"/> parameter; managed resources
        /// should only be disposed if <paramref name="disposing"/> is <c>true</c>
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_replyHub")]
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            Task.WhenAll(_subscriptionTasks).TryWait(TimeSpan.FromSeconds(30));
            if (disposing)
            {
                _cancellationTokenSource.TryDispose();
                _replyHub.TryDispose();
            }
        }
    }
}