using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using Platibus.Config;
using Platibus.Owin;
using Platibus.SampleApi;
using Platibus.SampleApi.Diagnostics;
using Platibus.SampleApi.Widgets;

[assembly: OwinStartup(typeof(Startup))]

namespace Platibus.SampleApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://localhost:44359/identity",
                RequiredScopes = new[] { "api" },
                ClientSecret = "secret"
            });
            
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            
            RegisterServices(containerBuilder);

            var container = containerBuilder.Build();
            app.UsePlatibusMiddleware(ConfigurePlatibus(container));

            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(httpConfiguration);
            app.UseWebApi(httpConfiguration);
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            var widgetRepository = new InMemoryWidgetRepository();
            containerBuilder.RegisterInstance(widgetRepository).As<IWidgetRepository>();
            containerBuilder.RegisterType<WidgetCreationCommandHandler>();
            containerBuilder.RegisterType<SimulatedRequestHandler>();
        }

        private static async Task<IOwinConfiguration> ConfigurePlatibus(IContainer container)
        {
            var configuration = await OwinConfigurationManager.LoadConfiguration();
            configuration.AddHandlingRules(container.Resolve<WidgetCreationCommandHandler>);
            configuration.AddHandlingRules(container.Resolve<SimulatedRequestHandler>);
            return configuration;
        }
    }
}
