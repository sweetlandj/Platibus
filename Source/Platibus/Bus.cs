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

using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.Serialization;
using Platibus.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IMessageQueueingService _messageQueueingService;
	    private readonly string _defaultContentType;
        private readonly SendOptions _defaultSendOptions;
        private readonly IDiagnosticService _diagnosticService;

        private readonly MessageMarshaller _messageMarshaller;
        private readonly MemoryCacheReplyHub _replyHub;
        private readonly IList<ISendRule> _sendRules;
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

            _messageMarshaller = new MessageMarshaller(
                configuration.MessageNamingService,
                configuration.SerializationService,
                configuration.DefaultContentType);

            _endpoints = configuration.Endpoints ?? EndpointCollection.Empty;
            _topics = configuration.Topics.ToList();
            _sendRules = configuration.SendRules.ToList();
            _handlingRules = configuration.HandlingRules.ToList();
            _subscriptions = configuration.Subscriptions.ToList();

            _diagnosticService = configuration.DiagnosticService;
            _messageHandler = new MessageHandler(_messageMarshaller, _diagnosticService);

            _transportService.MessageReceived += OnMessageReceived;

            _replyHub = new MemoryCacheReplyHub(_messageMarshaller, _diagnosticService, TimeSpan.FromMinutes(5));
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

                var queueListener = new MessageHandlingListener(this, _messageHandler, queueName, handlers, _diagnosticService);

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

        private Task SendMessage(Message message, IEndpointCredentials credentials,
            CancellationToken cancellationToken)
        {
            try
            {
                return _transportService.SendMessage(message, credentials, cancellationToken);
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
        public Task<ISentMessage> Send(object content, SendOptions options = default(SendOptions),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            if (content == null) throw new ArgumentNullException(nameof(content));

            var myOptions = options ?? _defaultSendOptions;
            var sent = DateTime.UtcNow;
            var prototypicalMessage = BuildMessage(content, null, myOptions);
            var endpoints = GetEndpointsForSend(prototypicalMessage);

            // Create the sent message before transporting it in order to ensure that the
            // reply stream is cached before any replies arrive.
            var sentMessage = _replyHub.CreateSentMessage(prototypicalMessage);
            
            // ReSharper disable once LoopCanBeConvertedToQuery
            var sendTasks = new List<Task>();
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

                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
                {
                    Message = addressedMessage,
                    Endpoint = endpointName
                }.Build());
            }

            var sentMessageSource = Task.WhenAll(sendTasks).GetCompletionSource(sentMessage, cancellationToken);
            return sentMessageSource.Task;
        }
        
        /// <inheritdoc />
        public Task<ISentMessage> Send(object content, EndpointName endpointName,
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
            var sentMessageSource = SendMessage(message, credentials, cancellationToken)
                .GetCompletionSource(sentMessage, cancellationToken);

            _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
            {
                Message = message,
                Endpoint = endpointName
            }.Build());

            return sentMessageSource.Task;
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
        public Task<ISentMessage> Send(object content, Uri endpointAddress,
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
            var sentMessageSource = SendMessage(message, credentials, cancellationToken)
                .GetCompletionSource(sentMessage, cancellationToken);

            _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
            {
                Message = message
            }.Build());

            return sentMessageSource.Task;
        }
        
        /// <inheritdoc />
        public Task Publish(object content, TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return PublishAsync(content, topic, cancellationToken)
                .GetCompletionSource(cancellationToken)
                .Task;
        }

        private async Task PublishAsync(object content, TopicName topic,
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
            var headers = new MessageHeaders(suppliedHeaders)
            {
                MessageId = MessageId.Generate(),
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

            return _messageMarshaller.Marshal(content, headers);
        }

        private IEnumerable<KeyValuePair<EndpointName, IEndpoint>> GetEndpointsForSend(Message message)
        {
            var matchingSendRules = _sendRules
                .Where(rule => rule.Specification.IsSatisfiedBy(message))
                .ToList();

            if (!matchingSendRules.Any())
            {
                _diagnosticService.Emit(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.NoMatchingSendRules)
                    {
                        Message = message
                    }.Build());

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

        internal Task SendReply(BusMessageContext messageContext, object replyContent,
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
            var sentMessageSource = SendMessage(replyMessage, credentials, cancellationToken)
                .GetCompletionSource(cancellationToken);

            _diagnosticService.Emit(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageSent)
                {
                    Message = replyMessage
                }.Build());

            return sentMessageSource.Task;
        }

        private Task OnMessageReceived(object sender, TransportMessageEventArgs args)
        {
            return HandleMessage(args.Message, args.Principal, args.CancellationToken);
        }

        /// <inheritdoc />
        public Task HandleMessage(Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            return InternalHandleMessage(message, principal, cancellationToken)
                .GetCompletionSource(cancellationToken)
                .Task;
        }

        private async Task InternalHandleMessage(Message message, IPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
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
                tasks.Add(HandleMessageImmediately(message, principal, cancellationToken));
            }
            else
            {
                // Message expiration handled in MessageHandlingListener
                var matchingRules = _handlingRules
                    .Where(r => r.Specification.IsSatisfiedBy(message))
                    .ToList();

                if (!matchingRules.Any())
                {
                    await _diagnosticService.EmitAsync(
                        new DiagnosticEventBuilder(this, DiagnosticEventType.NoMatchingHandlingRules)
                        {
                            Message = message
                        }.Build(), cancellationToken);
                    return;
                }

                var handlerQueues = matchingRules.Select(rule => rule.QueueName).Distinct();

                tasks.AddRange(handlerQueues.Select(queue => _messageQueueingService.EnqueueMessage(queue, message, principal, cancellationToken)));
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleMessageImmediately(Message message, IPrincipal senderPrincipal, CancellationToken cancellationToken)
        {
            var matchingRules = _handlingRules
                .Where(r => r.Specification.IsSatisfiedBy(message))
                .ToList();

            if (!matchingRules.Any())
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.NoMatchingHandlingRules)
                    {
                        Message = message
                    }.Build(), cancellationToken);

                throw new MessageNotAcknowledgedException("Message does not match any configured handling rules");
            }

            var messageContext = new BusMessageContext(this, message.Headers, senderPrincipal);
            var handlers = matchingRules.Select(rule => rule.MessageHandler).ToList();
            var autoAcknowledge = matchingRules.Any(rule => rule.QueueOptions?.AutoAcknowledge ?? false);
            
            await _messageHandler.HandleMessage(handlers, message, messageContext, _cancellationTokenSource.Token);

            var acknowledged = messageContext.MessageAcknowledged || autoAcknowledge;
            if (!acknowledged)
            {
                await _diagnosticService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.MessageNotAcknowledged)
                    {
                        Message = message
                    }.Build(), cancellationToken);

                throw new MessageNotAcknowledgedException("Message not acknowledged by any message handlers");
            }

            await _diagnosticService.EmitAsync(
                new DiagnosticEventBuilder(this, DiagnosticEventType.MessageAcknowledged)
                {
                    Message = message
                }.Build(), cancellationToken);
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

            await _replyHub.NotifyReplyReceived(message);
            await _replyHub.NotifyLastReplyReceived(message);
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