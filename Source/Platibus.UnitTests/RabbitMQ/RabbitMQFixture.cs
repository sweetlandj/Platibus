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
using Platibus.Diagnostics;
using Platibus.RabbitMQ;

namespace Platibus.UnitTests.RabbitMQ
{
    public class RabbitMQFixture : IDisposable
    {
        private bool _disposed;

        public IDiagnosticService DiagnosticService { get; } = new DiagnosticService();
        public Uri Uri { get; }
        public RabbitMQMessageQueueingService MessageQueueingService { get; }

        public RabbitMQFixture()
        {
            // docker run -it --rm --name rabbitmq -p 5682:5672 -p 15682:15672 rabbitmq:3-management
            Uri = new Uri("amqp://guest:guest@localhost:5682");

            var queueingOptions = new RabbitMQMessageQueueingOptions(Uri)
            {
                DiagnosticService = DiagnosticService,
                DefaultQueueOptions = new QueueOptions
                {
                    IsDurable = false
                }
            };

            MessageQueueingService = new RabbitMQMessageQueueingService(queueingOptions);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            MessageQueueingService.Dispose();
        }
    }
}
