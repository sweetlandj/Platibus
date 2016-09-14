using Microsoft.Owin;
using Owin;
using Platibus.Owin;
using Platibus.SampleWebApp;

[assembly: OwinStartup(typeof(Startup))]

namespace Platibus.SampleWebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UsePlatibusMiddleware();
        }
    }
}
