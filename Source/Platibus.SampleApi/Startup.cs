using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Platibus.AspNetCore;
using Platibus.Config;
using Platibus.SampleApi.Diagnostics;
using Platibus.SampleApi.Widgets;

namespace Platibus.SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:44335/identity";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };

                    //options.Events = new JwtBearerEvents
                    //{
                    //    OnAuthenticationFailed = context =>
                    //    {
                    //        var e = context.Exception;
                    //        return Task.CompletedTask;
                    //    },
                    //    OnTokenValidated = context =>
                    //    {
                    //        var t = context.SecurityToken;
                    //        var p = context.Principal;
                    //        return Task.CompletedTask;
                    //    }
                    //};
                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("https://localhost:44335")
                        .WithMethods("GET")
                        .AllowAnyHeader());
            });

            services.AddMvc();

            // Widget services
            services.AddSingleton<IWidgetRepository>(new InMemoryWidgetRepository());
            services.AddSingleton<WidgetCreationCommandHandler>();
            services.AddSingleton<SimulatedRequestHandler>();
            services.AddSingleton<AspNetCoreLoggingSink>();

            // Platibus services
            var serviceProvider = services.BuildServiceProvider();
            services.AddPlatibus(configuration =>
            {
                var sink = serviceProvider.GetService<AspNetCoreLoggingSink>();
                configuration.DiagnosticService.AddSink(sink);
                configuration.AddHandlingRules(serviceProvider.GetService<WidgetCreationCommandHandler>);
                configuration.AddHandlingRules(serviceProvider.GetService<SimulatedRequestHandler>);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseCors("AllowSpecificOrigin");
            app.UsePlatibusMiddleware();
            app.UseMvc();
        }
    }
}
