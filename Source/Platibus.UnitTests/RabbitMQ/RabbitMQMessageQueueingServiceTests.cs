﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.RabbitMQ;
using RabbitMQ.Client;

namespace Platibus.UnitTests.RabbitMQ
{
    public class RabbitMQMessageQueueingServiceTests : MessageQueueingServiceTests<RabbitMQMessageQueueingService>
    {
        private readonly Uri _uri;

        public RabbitMQMessageQueueingServiceTests()
            : this(RabbitMQCollectionFixture.Instance)
        {
        }

        public RabbitMQMessageQueueingServiceTests(RabbitMQCollectionFixture fixture)
            : base(fixture.MessageQueueingService)
        {
            _uri = fixture.Uri;
        }

        protected override async Task GivenExistingQueuedMessage(QueueName queueName, Message message, IPrincipal principal)
        {
            var connectionFactory = new ConnectionFactory { Uri = _uri.ToString() };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // We have to declare the queue as a persistence queue because this is 
                // called before the queue is created by the RabbitMQQueueingService
                var deadLetterExchange = queueName.GetDeadLetterExchangeName();
                var queueArgs = new Dictionary<string, object>
                {
                    {"x-dead-letter-exchange", deadLetterExchange}
                };
                channel.ExchangeDeclare(deadLetterExchange, "direct", true, false, null);
                channel.QueueDeclare(queueName, true, false, false, queueArgs);
                await RabbitMQHelper.PublishMessage(message, Thread.CurrentPrincipal, channel, queueName);
            }
        }
        
        protected override Task<bool> MessageQueued(QueueName queueName, Message message)
        {
            var messagesInQueue = GetQueueDepth(queueName) > 0;
            return Task.FromResult(messagesInQueue);
        }

        protected override Task<bool> MessageDead(QueueName queueName, Message message)
        {
            // TODO: Bind new queue to dead letter exchange and check queue
            return Task.FromResult(true);
        }

        protected override Task AssertMessageStillQueuedForRetry(QueueName queue, Message message)
        {
            var retryQueue = queue.GetRetryQueueName();
            var inRetryQueue = GetQueueDepth(retryQueue) > 0;
            return Task.FromResult(inRetryQueue);
        }

        private uint GetQueueDepth(QueueName queueName)
        {
            var connectionFactory = new ConnectionFactory { Uri = _uri.ToString() };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var result = channel.QueueDeclarePassive(queueName);
                return result.MessageCount;
            }
        }
    }
}
