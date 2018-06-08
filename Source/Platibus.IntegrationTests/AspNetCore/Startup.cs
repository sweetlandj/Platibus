#if NETCOREAPP2_0
using Microsoft.AspNetCore.Builder;
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
            services.ConfigurePlatibus(_configuration);
            services.AddPlatibus();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UsePlatibusMiddleware();
        }
    }
}

#endif