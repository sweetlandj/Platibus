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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.Journaling;
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Hosts for a bus instance whose queueing and transport are based on RabbitMQ queues
    /// </summary>
    public class RabbitMQHost : ITransportService, IQueueListener, IDisposable
    {
        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/> based on the configuration
        /// in the named configuration section.
        /// </summary>
        /// <param name="configSectionName">The configuration section containing the
        /// settings for this RabbitMQ hostinstance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the Rabbit MQ host initialization process</param>
        /// <param name="diagnosticService">The service through which diagnosic events are reported
        /// and processed</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="RabbitMQHostConfigurationSection"/>
        public static async Task<RabbitMQHost> Start(string configSectionName = "platibus.rabbitmq",
            CancellationToken cancellationToken = default(CancellationToken), 
            IDiagnosticService diagnosticService = null)
        {
            var configManager = new RabbitMQHostConfigurationManager();
            var configuration = new RabbitMQHostConfiguration(diagnosticService);
            await configManager.Initialize(configuration, configSectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            return await Start(configuration, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="RabbitMQHost"/> based on the specified
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration for this HTTP server instance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the HTTP server initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="RabbitMQHostConfigurationSection"/>
        public static async Task<RabbitMQHost> Start(IRabbitMQHostConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var server = new RabbitMQHost(configuration);
            await server.Init(cancellationToken);
            return server;
        }

        private const string InboxQueueName = "inbox";

        private readonly IConnectionManager _connectionManager;
        private readonly Uri _baseUri;
        private readonly Encoding _encoding;
        private readonly QueueOptions _defaultQueueOptions;
        private readonly RabbitMQQueue _inboundQueue;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;
        private readonly ISecurityTokenService _securityTokenService;
        private readonly IDiagnosticService _diagnosticService;
        private readonly ConcurrentDictionary<SubscriptionKey, RabbitMQQueue> _subscriptions = new ConcurrentDictionary<SubscriptionKey, RabbitMQQueue>(); 

        private bool _disposed;

        /// <summary>
        /// The hosted bus instance
        /// </summary>
        public Bus Bus { get; }

        private RabbitMQHost(IRabbitMQHostConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _diagnosticService = configuration.DiagnosticService;
            _baseUri = configuration.BaseUri.WithoutTrailingSlash();
            _connectionManager = new ConnectionManager();
            _encoding = configuration.Encoding ?? Encoding.UTF8;

            _defaultQueueOptions = new QueueOptions
            {
                AutoAcknowledge = configuration.AutoAcknowledge,
                MaxAttempts = configuration.MaxAttempts,
                ConcurrencyLimit = configuration.ConcurrencyLimit,
                RetryDelay = configuration.RetryDelay,
                IsDurable = configuration.IsDurable
            };

            _securityTokenService = configuration.SecurityTokenService;
            _messageJournal = configuration.MessageJournal;
            _messageQueueingService = new RabbitMQMessageQueueingService(_baseUri, _defaultQueueOptions,
                _connectionManager, _encoding, _securityTokenService, _diagnosticService);
            
            var connection = _connectionManager.GetConnection(_baseUri);
            using (var channel = connection.CreateModel())
            {
                foreach (var topicName in configuration.Topics)
                {
                    var exchangeName = topicName.GetTopicExchangeName();
                    channel.ExchangeDeclare(exchangeName, "fanout", configuration.IsDurable, false, new Dictionary<string, object>());
                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQExchangeDeclared)
                        {
                            Detail = "Fanout exchange declared for topic",
                            Exchange = exchangeName,
                            Topic = topicName,
                            ChannelNumber = channel.ChannelNumber
                        }.Build());
                }
            }

            _inboundQueue = new RabbitMQQueue(InboxQueueName, this, connection, _securityTokenService, _encoding,
                _defaultQueueOptions, _diagnosticService);

            Bus = new Bus(configuration, configuration.BaseUri, this, _messageQueueingService);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await Bus.Init(cancellationToken);
            _inboundQueue.Init();
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
            // For now, allow exceptions to propagate and be handled by the RabbitMQQueue
            await Bus.HandleMessage(message, null);
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
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            var destination = message.Headers.Destination;
            var connection = _connectionManager.GetConnection(destination);
            using (var channel = connection.CreateModel())
            {
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, InboxQueueName);
                await _diagnosticService.EmitAsync(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageDelivered)
                    {
                        Message = message,
                        ChannelNumber = channel.ChannelNumber,
                        Queue = InboxQueueName
                    }.Build(), cancellationToken);
            }
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
            CheckDisposed();
            var publisherTopicExchange = topicName.GetTopicExchangeName();
            var connection = _connectionManager.GetConnection(_baseUri);
            using (var channel = connection.CreateModel())
            {
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, null, publisherTopicExchange);
                await _diagnosticService.EmitAsync(
                    new RabbitMQEventBuilder(this, DiagnosticEventType.MessageDelivered)
                    {
                        Message = message,
                        ChannelNumber = channel.ChannelNumber,
                        Exchange = publisherTopicExchange
                    }.Build(), cancellationToken);
            }
        }

        /// <summary>
        /// Subscribes to messages published to the specified <paramref name="topicName"/>
        /// by the application at the provided <paramref name="endpoint"/>.
        /// </summary>
        /// <param name="endpoint">The publishing endpoint</param>
        /// <param name="topicName">The name of the topic to which the caller is
        ///     subscribing.</param>
        /// <param name="ttl">(Optional) The Time To Live (TTL) for the subscription
        ///     on the publishing application if it is not renewed.</param>
        /// <param name="cancellationToken">A token used by the caller to
        ///     indicate if and when the subscription should be canceled.</param>
        /// <returns>Returns a long-running task that will be completed when the 
        /// subscription is canceled by the caller or a non-recoverable error occurs.</returns>
        public Task Subscribe(IEndpoint endpoint, TopicName topicName, TimeSpan ttl, CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();

            var publisherUri = endpoint.Address;
            var subscriptionQueueName = _baseUri.GetSubscriptionQueueName(topicName);
            var subscriptionKey = new SubscriptionKey(publisherUri, subscriptionQueueName);
            _subscriptions.GetOrAdd(subscriptionKey, _ => BindSubscriptionQueue(topicName, publisherUri, subscriptionQueueName, cancellationToken));
            return Task.FromResult(true);
        }

        private RabbitMQQueue BindSubscriptionQueue(TopicName topicName, Uri publisherUri, string subscriptionQueueName, CancellationToken cancellationToken)
        {
            var publisherTopicExchange = topicName.GetTopicExchangeName();           
            var attempts = 0;
            const int maxAttempts = 10;
            var retryDelay = TimeSpan.FromSeconds(5);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var connection = _connectionManager.GetConnection(publisherUri))
                    using (var channel = connection.CreateModel())
                    {
                        attempts++;
                        channel.ExchangeDeclarePassive(publisherTopicExchange);

                        var subscriptionQueue = new RabbitMQQueue(subscriptionQueueName, this, connection,
                            _securityTokenService, _encoding, _defaultQueueOptions, _diagnosticService);

                        subscriptionQueue.Init();

                        channel.QueueBind(subscriptionQueueName, publisherTopicExchange, "", null);

                        _diagnosticService.Emit(
                            new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueBound)
                            {
                                Detail = "Subscription queue bound to topic exchange",
                                Queue = subscriptionQueueName,
                                Exchange = publisherTopicExchange,
                                Topic = topicName,
                                ChannelNumber = channel.ChannelNumber
                            }.Build());

                        return subscriptionQueue;
                    }
                }
                catch (Exception ex)
                {
                    if (attempts >= maxAttempts)
                    {
                        throw;
                    }

                    _diagnosticService.Emit(
                        new RabbitMQEventBuilder(this, RabbitMQEventType.RabbitMQQueueBindError)
                        {
                            Detail = "Error binding subscription queue to topic exchange (attempt " + attempts + " of " + maxAttempts + ").  Retrying in " + retryDelay,
                            Queue = subscriptionQueueName,
                            Exchange = publisherTopicExchange,
                            Topic = topicName,
                            Exception = ex
                        }.Build());
                    
                    Task.Delay(retryDelay, cancellationToken).Wait(cancellationToken);
                }
            }

            throw new OperationCanceledException();
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~RabbitMQHost()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var subscriptionQueue in _subscriptions)
                {
                    subscriptionQueue.Value.Dispose();
                }
                _inboundQueue.Dispose();
                Bus.Dispose();

                var disposableMessageQueueingService = _messageQueueingService as IDisposable;
                if (disposableMessageQueueingService != null)
                {
                    disposableMessageQueueingService.Dispose();
                }

                var disposableMessageJournal = _messageJournal as IDisposable;
                if (disposableMessageJournal != null)
                {
                    disposableMessageJournal.Dispose();
                }

                var disposableConnectionManager = _connectionManager as IDisposable;
                if (disposableConnectionManager != null)
                {
                    disposableConnectionManager.Dispose();
                }
            }
        }

        private class SubscriptionKey : IEquatable<SubscriptionKey>
        {
            private readonly Uri _publisherUri;
            private readonly QueueName _subscriptionQueueName;

            public SubscriptionKey(Uri publisherUri, QueueName subscriptionQueueName)
            {
                if (publisherUri == null) throw new ArgumentNullException("publisherUri");
                if (subscriptionQueueName == null) throw new ArgumentNullException("subscriptionQueueName");
                _publisherUri = publisherUri;
                _subscriptionQueueName = subscriptionQueueName;
            }

            public bool Equals(SubscriptionKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(_publisherUri, other._publisherUri) && Equals(_subscriptionQueueName, other._subscriptionQueueName);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SubscriptionKey);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_publisherUri != null ? _publisherUri.GetHashCode() : 0)*397) ^ (_subscriptionQueueName != null ? _subscriptionQueueName.GetHashCode() : 0);
                }
            }

            public static bool operator ==(SubscriptionKey left, SubscriptionKey right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(SubscriptionKey left, SubscriptionKey right)
            {
                return !Equals(left, right);
            }
        }
    }
}
