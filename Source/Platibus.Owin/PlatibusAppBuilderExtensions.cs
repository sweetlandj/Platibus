using System;
using System.Threading.Tasks;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Platibus.Owin
{
    public static class PlatibusAppBuilderExtensions
    {
        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, string sectionName = null)
        {
            if (app == null) throw new ArgumentNullException("app");
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(sectionName), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, IOwinConfiguration configuration)
        {
            if (app == null) throw new ArgumentNullException("app");
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(configuration), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, Task<IOwinConfiguration> configuration)
        {
            if (app == null) throw new ArgumentNullException("app");
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(configuration), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, PlatibusMiddleware middleware, bool disposeMiddleware = false)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (middleware == null) throw new ArgumentNullException("app");

            if (disposeMiddleware)
            {
                var appProperties = new AppProperties(app.Properties);
                appProperties.OnAppDisposing.Register(middleware.Dispose);
            }

            return app.Use(middleware.Invoke);
        }
    }
}
