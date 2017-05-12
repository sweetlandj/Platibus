using System;
using Platibus.RabbitMQ;

namespace Platibus.UnitTests.RabbitMQ
{
    public class RabbitMQFixture : IDisposable
    {
        private readonly Uri _uri;
        private readonly RabbitMQMessageQueueingService _messageQueueingService;

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
            _messageQueueingService.TryDispose();
        }

    }
}
