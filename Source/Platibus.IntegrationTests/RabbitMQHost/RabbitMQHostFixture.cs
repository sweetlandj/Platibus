using System;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.RabbitMQHost
{
    public class RabbitMQHostFixture : IDisposable
    {
        private readonly Task<RabbitMQ.RabbitMQHost> _sendingHost;
        private readonly Task<RabbitMQ.RabbitMQHost> _receivingHost;

        public Task<IBus> Sender
        {
            get { return _sendingHost.ContinueWith(hostTask => (IBus) hostTask.Result.Bus); }
        }

        public Task<IBus> Receiver
        {
            get { return _receivingHost.ContinueWith(hostTask => (IBus) hostTask.Result.Bus); }
        }

        public RabbitMQHostFixture()
        {
            _sendingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq0");
            _receivingHost = RabbitMQ.RabbitMQHost.Start("platibus.rabbitmq1");
        }

        public void Dispose()
        {
            Task.WhenAll(
                    _sendingHost.ContinueWith(t => t.Result.TryDispose()),
                    _receivingHost.ContinueWith(t => t.Result.TryDispose()))
                .Wait(TimeSpan.FromSeconds(10));
        }

    }
}
