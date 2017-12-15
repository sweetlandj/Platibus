using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Platibus.SampleApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("https://localhost:44313/")
                .Build();
    }
}
