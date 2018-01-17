using Microsoft.Extensions.CommandLineUtils;

namespace Platibus.SampleConsoleApp
{
    public class ListenCommand : RabbitMQHostCommand
    {
        public static void Configure(CommandLineApplication app)
        {
            app.HelpOption("-h|-?|--help");

            var idOption = app.Option(
                "-n|--node",
                "The node ID",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var id = idOption.Value();
                var command = new ListenCommand(id);
                return command.Execute();
            });
        }

        public ListenCommand(string id) : base(id)
        {  
        }
    }
}
