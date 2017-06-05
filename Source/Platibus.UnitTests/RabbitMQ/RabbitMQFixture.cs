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
using Platibus.RabbitMQ;

namespace Platibus.UnitTests.RabbitMQ
{
    public class RabbitMQFixture : IDisposable
    {
        private readonly Uri _uri;
        private readonly RabbitMQMessageQueueingService _messageQueueingService;

        private bool _disposed;

        public Uri Uri { get { return _uri; } }
        public RabbitMQMessageQueueingService MessageQueueingService { get { return _messageQueueingService; } }

        public RabbitMQFixture()
        {
            _uri = new Uri("amqp://test:test@localhost:5672/test");
            _messageQueueingService = new RabbitMQMessageQueueingService(_uri, new QueueOptions
            {
                IsDurable = false
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_messageQueueingService")]
        protected virtual void Dispose(bool disposing)
        {
            _messageQueueingService.TryDispose();
        }
    }
}
