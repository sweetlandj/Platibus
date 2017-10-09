// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Diagnostics;
using Platibus.RabbitMQ;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Platibus.UnitTests.RabbitMQ
{
    [Trait("Category", "UnitTests")]
    [Trait("Dependency", "RabbitMQ")]
    [Collection(RabbitMQCollection.Name)]
    public class RabbitMQMessageQueueingServiceTests : MessageQueueingServiceTests<RabbitMQMessageQueueingService>, IDisposable
    {
        private readonly Uri _uri;
        private readonly TestOutputHelperSink _diagnosticSink;
        
        public RabbitMQMessageQueueingServiceTests(RabbitMQFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture.MessageQueueingService)
        {
            _uri = fixture.Uri;
            _diagnosticSink = new TestOutputHelperSink(outputHelper);
            DiagnosticService.DefaultInstance.AddSink(_diagnosticSink);
        }

        public void Dispose()
        {
            DiagnosticService.DefaultInstance.RemoveSink(_diagnosticSink);
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
                channel.ExchangeDeclare(deadLetterExchange, "direct", false, false, null);
                channel.QueueDeclare(queueName, false, false, false, queueArgs);
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
