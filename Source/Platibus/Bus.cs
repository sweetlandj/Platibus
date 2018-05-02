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
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.Serialization;

namespace Platibus
{
    /// <inheritdoc cref="IBus"/>
    /// <summary>
    /// Default <see cref="T:Platibus.IBus" /> implementation
    /// </summary>
    public class Bus : IBus, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IEndpointCollection _endpoints;
        private readonly IList<Task> _subscriptionTasks = new List<Task>();
        private readonly IList<IHandlingRule> _handlingRules;
        private readonly IMessageNamingService _messageNamingService;
        private readonly IMessageJournal _messageJournal;
        private readonly IMessageQueueingService _messageQueueingService;
	    private readonly string _defaultContentType;
        private readonly SendOptions _defaultSendOptions;
        private readonly IDiagnosticService _diagnosticService;

        private readonly MemoryCacheReplyHub _replyHub = new MemoryCacheReplyHub(TimeSpan.FromMinutes(5));
        private readonly IList<ISendRule> _sendRules;
        private readonly ISerializationService _serializationService;
        private readonly IList<ISubscription> _subscriptions;
        private readonly IList<TopicName> _topics;
        private readonly Uri _baseUri;
        private readonly ITransportService _transportService;
        private readonly MessageHandler _messageHandler;

        private bool _disposed;

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
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Validate the provided configuration and throw exceptions for missing or invalid
            // configurations
            configuration.Validate();

            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            _transportService = transportService ?? throw new ArgumentNullException(nameof(transportService));
            _messageQueueingService = messageQueueingService ?? throw new ArgumentNullException(nameof(messageQueueingService));
            _defaultContentType = string.IsNullOrWhiteSpace(configuration.DefaultContentType)
                ? "application/json"
                : configuration.DefaultContentType;

            _defaultSendOptions = configuration.DefaultSendOptions ?? new SendOptions();

            _messageJournal = configuration.MessageJournal;
            _messageNamingService = configuration.MessageNamingService;
            _serializationService = configuration.SerializationService;

            _endpoints = configuration.Endpoints ?? EndpointCollection.Empty;
            _topics = configuration.Topics.ToList();
            _sendRules = configuration.SendRules.ToList();
            _handlingRules = configuration.HandlingRules.ToList();
            _subscriptions = configuration.Subscriptions.ToList();

            _diagnosticService = configuration.DiagnosticService;
            _messageHandler = new MessageHandler(_messageNamingService, _serializationService, _diagnosticService);
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

                var queueListener = new MessageHandlingListener(this, _messageNamingService,
                    _serializationService, queueName, handlers, _diagnosticService);

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

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.BusInitialized).Build(),
                cancellationToken);
        }

        private static int QueueOptionPrecedence(QueueOptions queueOptions)
        {
            // Prefer overrides to default options
            return queueOptions == null ? 1 : 0;
        }

        private async Task SendMessage(Message message, IEndpointCredentials credentials,
            CancellationToken cancellationToken)
        {
            try
            {
                await _transportService.SendMessage(message, credentials, cancellationToken);
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSendFailed)
                {
                    Message = message,
                    Exception = ex
                }.Build());
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ISentMessage> Send(object content, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException(nameof(content));

            var myOptions = options ?? _defaultSendOptions;
            var sent = DateTime.UtcNow;
            var prototypicalMessage = BuildMessage(content, null, myOptions);
            var endpoints = GetEndpointsForSend(prototypicalMessage);

            var sendTasks = new List<Task>();

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(prototypicalMessage);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var kvp in endpoints)
            {
                var endpointName = kvp.Key;
                var endpoint = kvp.Value;
                var credentials = endpoint.Credentials;
                if (myOptions.Credentials != null)
                {
                    credentials = myOptions.Credentials;
                }

                var perEndpointHeaders = new MessageHeaders(prototypicalMessage.Headers)
                {
                    Destination = endpoint.Address,
                    Sent = sent
                };

                var addressedMessage = new Message(perEndpointHeaders, prototypicalMessage.Content);
                sendTasks.Add(SendMessage(addressedMessage, credentials, cancellationToken));

                await _diagnosticService.EmitAsync(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
                {
                    Message = addressedMessage,
                    Endpoint = endpointName
                }.Build(), cancellationToken);
            }
            await Task.WhenAll(sendTasks);
            return sentMessage;
        }
        
        /// <inheritdoc />
        public async Task<ISentMessage> Send(object content, EndpointName endpointName,
            SendOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (endpointName == null) throw new ArgumentNullException(nameof(endpointName));

            var myOptions = options ?? _defaultSendOptions;
            var endpoint = _endpoints[endpointName];
            var credentials = endpoint.Credentials;
            if (myOptions.Credentials != null)
            {
                credentials = myOptions.Credentials;
            }

            var headers = new MessageHeaders
            {
                Destination = endpoint.Address,
                Sent = DateTime.UtcNow
            };
            var message = BuildMessage(content, headers, myOptions);

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(message);
            await SendMessage(message, credentials, cancellationToken);

            await _diagnosticService.EmitAsync(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
            {
                Message = message,
                Endpoint = endpointName
            }.Build(), cancellationToken);

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

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (endpointAddress == null) throw new ArgumentNullException(nameof(endpointAddress));

            var myOptions = options ?? _defaultSendOptions;
            var headers = new MessageHeaders
            {
                Destination = endpointAddress,
                Sent = DateTime.UtcNow
            };

            var message = BuildMessage(content, headers, myOptions);
            var credentials = myOptions.Credentials;

            if (credentials == null && _endpoints.TryGetEndpointByAddress(endpointAddress, out IEndpoint knownEndpoint))
            {
                credentials = knownEndpoint.Credentials;
            }

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(message);
            await SendMessage(message, credentials, cancellationToken);

            await _diagnosticService.EmitAsync(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
            {
                Message = message
            }.Build(), cancellationToken);

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
                Published = DateTime.UtcNow
            };

            var message = BuildMessage(content, prototypicalHeaders, null);
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Published, cancellationToken);
            }

            await _transportService.PublishMessage(message, topic, cancellationToken);
            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessagePublished)
                {
                    Message = message,
                    Topic = topic
                }.Build(), cancellationToken);
        }

        private Message BuildMessage(object content, IMessageHeaders suppliedHeaders, SendOptions options)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var messageName = _messageNamingService.GetNameForType(content.GetType());

            var headers = new MessageHeaders(suppliedHeaders)
            {
                MessageId = MessageId.Generate(),
                MessageName = messageName,
                Origination = _baseUri,
                Synchronous = options != null && options.Synchronous
            };

            if (options != null && options.TTL > TimeSpan.Zero)
            {
                headers.Expires = DateTime.UtcNow.Add(options.TTL);
            }

            var contentType = options?.ContentType;
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
            if (messageContext == null) throw new ArgumentNullException(nameof(messageContext));
            if (replyContent == null) throw new ArgumentNullException(nameof(replyContent));

            IEndpointCredentials credentials = null;
            var replyTo = messageContext.Headers.ReplyTo ?? messageContext.Headers.Origination;
            if (_endpoints.TryGetEndpointByAddress(replyTo, out IEndpoint replyToEndpoint))
            {
                credentials = replyToEndpoint.Credentials;
            }

            var headers = new MessageHeaders
            {
                Destination = messageContext.Headers.Origination,
                RelatedTo = messageContext.Headers.MessageId
            };
            var replyMessage = BuildMessage(replyContent, headers, options);
            await SendMessage(replyMessage, credentials, cancellationToken);

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
                {
                    Message = replyMessage
                }.Build(), cancellationToken);
        }

        /// <summary>
        /// Called by the host when a new message arrives to handle the message
        /// </summary>
        /// <param name="message">The new message</param>
        /// <param name="principal">The sender principal</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to request cancellation of the message handling request</param>
        /// <returns>Returns a task that completes when message handling is complete</returns>
        public async Task HandleMessage(Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_messageJournal != null)
            {
                await _messageJournal.Append(message, MessageJournalCategory.Received, cancellationToken);
            }

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageReceived)
                {
                    Message = message
                }.Build(), cancellationToken);

            var tasks = new List<Task>();
            var isReply = message.Headers.RelatedTo != default(MessageId);
            if (isReply)
            {
                tasks.Add(NotifyReplyReceived(message));
            }

            if (message.Headers.Synchronous)
            {
                tasks.Add(HandleMessageImmediately(message, principal));
            }
            else
            {
                // Message expiration handled in MessageHandlingListener
                tasks.AddRange(_handlingRules
                    .Where(r => r.Specification.IsSatisfiedBy(message))
                    .Select(rule => rule.QueueName)
                    .Distinct()
                    .Select(q => _messageQueueingService.EnqueueMessage(q, message, principal, cancellationToken)));
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
            
            await _messageHandler.HandleMessage(handlers, message, messageContext, _cancellationTokenSource.Token);
            
            if (!messageContext.MessageAcknowledged)
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                    {
                        Message = message
                    }.Build());

                throw new MessageNotAcknowledgedException();
            }

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageAcknowledged)
                {
                    Message = message
                }.Build());
        }

        private async Task NotifyReplyReceived(Message message)
        {
            // TODO: Handle special "last reply" message.  Presently we only support a single reply.
            // This will probably be a special function of the ITransportService and will likely
            // be, for example, an empty POST to {baseUri}/message/{messageId}/lastReply, which
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
        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            Task.WhenAll(_subscriptionTasks).Wait(TimeSpan.FromSeconds(10));
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
                _replyHub.Dispose();
            }
        }
    }
}