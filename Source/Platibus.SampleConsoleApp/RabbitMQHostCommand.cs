using Platibus.Config;
using Platibus.Multicast;
using Platibus.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleConsoleApp
{
    public class RabbitMQHostCommand
    {
        protected readonly string Id;

        public RabbitMQHostCommand(string id)
        {
            Id = string.IsNullOrWhiteSpace(id) ? NodeId.Generate().ToString() : id;
        }

        public async Task<int> Execute()
        {
            try
            {
                var configuration = new RabbitMQHostConfiguration();
                var configurationManager = new RabbitMQHostConfigurationManager();
                await configurationManager.Initialize(configuration);

                var pingHandler = new PingHandler(Id);
                configuration.AddHandlingRules(pingHandler, (ht, mt) => "Pings");

                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                using (var host = RabbitMQHost.Start(configuration, cancellationToken))
                {
                    Console.WriteLine($"[{Id}] Listening...");
                    await Execute(host, cancellationToken);
                    await UserInterrupt.Wait(cancellationToken);
                }
                cancellationTokenSource.Cancel();
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            Console.WriteLine($"[{Id}] Stopping...");
            return 0;
        }

        protected virtual Task Execute(RabbitMQHost host, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
