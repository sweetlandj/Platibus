#if NETCOREAPP2_0
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Platibus.AspNetCore;

namespace Platibus.IntegrationTests.AspNetCore
{
    public class Startup
    {
        private readonly AspNetCoreConfiguration _configuration;

        public Startup(AspNetCoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<AspNetCoreLoggingSink>();

            // Platibus services
            var serviceProvider = services.BuildServiceProvider();
            var sink = serviceProvider.GetService<AspNetCoreLoggingSink>();
            _configuration.DiagnosticService.AddSink(sink);
            services.AddPlatibusServices(_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePlatibusMiddleware();
        }
    }
}

#endif