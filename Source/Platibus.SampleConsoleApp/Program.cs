using Microsoft.Extensions.CommandLineUtils;

namespace Platibus.SampleConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.HelpOption("-h|-?|--help");
            app.Command("send", SendCommand.Configure);
            app.Command("listen", ListenCommand.Configure);

            // Listen by default
            ListenCommand.Configure(app);

            app.Execute(args);
        }
    }
}
