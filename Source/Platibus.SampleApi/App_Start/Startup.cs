using System.Threading.Tasks;
using Autofac;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using Platibus.Config;
using Platibus.Owin;
using Platibus.SampleApi;
using Platibus.SampleApi.Controllers;

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
            RegisterServices(containerBuilder);

            var container = containerBuilder.Build();
            app.UsePlatibusMiddleware(ConfigurePlatibus(container));
        }

        private static void RegisterServices(ContainerBuilder containerBuilder)
        {
            var receivedMessageRepository = new ReceivedMessageRepository();
            receivedMessageRepository.Init();
            containerBuilder.RegisterInstance(receivedMessageRepository);
            containerBuilder.RegisterType<TestMessageHandler>();
        }

        private static async Task<IOwinConfiguration> ConfigurePlatibus(IContainer container)
        {
            var configuration = await OwinConfigurationManager.LoadConfiguration();
            configuration.AddHandlingRules(container.Resolve<TestMessageHandler>);
            return configuration;
        }
    }
}
