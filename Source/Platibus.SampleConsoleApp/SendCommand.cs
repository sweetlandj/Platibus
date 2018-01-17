using Microsoft.Extensions.CommandLineUtils;
using Platibus.RabbitMQ;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleConsoleApp
{
    public class SendCommand : RabbitMQHostCommand
    {
        public static void Configure(CommandLineApplication app)
        {
            app.HelpOption("-h|-?|--help");

            var countOption = app.Option(
                "-c|--count", 
                "The number of pings to send through the system",
                CommandOptionType.SingleValue);

            var idOption = app.Option(
                "-n|--node",
                "The node ID",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                int.TryParse(countOption.Value(), out var count);
                var id = idOption.Value();
                var command = new SendCommand(count, id);
                return command.Execute();
            });
        }

        private readonly int _count;

        public SendCommand(int count, string id) : base(id)
        {
            _count = count;
        }

        protected override async Task Execute(RabbitMQHost host, CancellationToken cancellationToken)
        {
            var ping = new Ping(Id, _count);
            await host.Bus.Send(ping, cancellationToken: cancellationToken);
        }
    }
}
