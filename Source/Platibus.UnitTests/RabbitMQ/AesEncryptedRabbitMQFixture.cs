using System;
using System.Threading;
using Platibus.Diagnostics;
using Platibus.RabbitMQ;
using Platibus.Security;
using Platibus.UnitTests.Security;

namespace Platibus.UnitTests.RabbitMQ
{
    public class AesEncryptedRabbitMQFixture : IDisposable
    {
        private bool _disposed;

        public IDiagnosticService DiagnosticService { get; } = new DiagnosticService();
        public Uri Uri { get; }
        public AesMessageEncryptionService MessageEncryptionService { get; }
        public RabbitMQMessageQueueingService MessageQueueingService { get; }

        public AesEncryptedRabbitMQFixture()
        {
            // docker run -it --rm --name rabbitmq -p 5682:5672 -p 15682:15672 rabbitmq:3-management
            Uri = new Uri("amqp://guest:guest@localhost:5682");

            WaitForRabbitMQ(Uri);

            var encryptionOptions = new AesMessageEncryptionOptions(KeyGenerator.GenerateAesKey())
            {
                DiagnosticService = DiagnosticService
            };
            MessageEncryptionService = new AesMessageEncryptionService(encryptionOptions);

            var queueingOptions = new RabbitMQMessageQueueingOptions(Uri)
            {
                DiagnosticService = DiagnosticService,
                DefaultQueueOptions = new QueueOptions
                {
                    IsDurable = false
                },
                MessageEncryptionService = MessageEncryptionService
            };
            MessageQueueingService = new RabbitMQMessageQueueingService(queueingOptions);
        }


        private static void WaitForRabbitMQ(Uri uri)
        {
            using (var connectionManager = new ConnectionManager())
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var connection = connectionManager.GetConnection(uri);
                        if (connection.IsOpen) return;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            throw new TimeoutException("RabbitMQ not available");
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