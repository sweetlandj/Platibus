using System;
using NUnit.Framework;
using Platibus.RabbitMQ;

namespace Platibus.UnitTests.RabbitMQ
{
    [SetUpFixture]
    public class RabbitMQCollectionFixture
    {
        public static RabbitMQCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new RabbitMQCollectionFixture();
        }

        [TearDown]
        public void TearDown()
        {
            if (Instance != null)
            {
                Instance._messageQueueingService.TryDispose();
            }
        }

        private readonly Uri _uri;
        private readonly RabbitMQMessageQueueingService _messageQueueingService;

        public Uri Uri { get { return _uri; } }
        public RabbitMQMessageQueueingService MessageQueueingService { get { return _messageQueueingService; } }

        public RabbitMQCollectionFixture()
        {
            _uri = new Uri("amqp://test:test@localhost:5672/test");
            _messageQueueingService = new RabbitMQMessageQueueingService(_uri, new QueueOptions
            {
                IsDurable = false
            });
        }
    }
}
