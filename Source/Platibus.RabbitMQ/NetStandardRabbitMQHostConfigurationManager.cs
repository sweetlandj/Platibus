#if NETSTANDARD2_0 || NET461

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Platibus.Config;
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Factory class used to initialize <see cref="RabbitMQHostConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class NetStandardRabbitMQHostConfigurationManager : NetStandardConfigurationManager<RabbitMQHostConfiguration>
    {
        /// <inheritdoc />
        public override async Task Initialize(RabbitMQHostConfiguration platibusConfiguration, string configSectionName = null)
        {
            var diagnosticsService = platibusConfiguration.DiagnosticService;
            if (string.IsNullOrWhiteSpace(configSectionName))
            {
                configSectionName = "platibus.rabbitmq";
                await diagnosticsService.EmitAsync(
                    new DiagnosticEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                    {
                        Detail = "Using default configuration section \"" + configSectionName + "\""
                    }.Build());
            }

            var configSection = LoadConfigurationSection(configSectionName, diagnosticsService);
            await Initialize(platibusConfiguration, configSection);
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes the supplied HTTP server <paramref name="platibusConfiguration" /> based on the
        /// properties of the provided <paramref name="configuration" />
        /// </summary>
        /// <param name="platibusConfiguration">The configuration to initialize</param>
        /// <param name="configuration">The configuration section whose properties are to be used
        /// to initialize the <paramref name="platibusConfiguration" /></param>
        /// <returns>Returns a task that completes when the configuration has been initialized</returns>
        public override async Task Initialize(RabbitMQHostConfiguration platibusConfiguration, IConfiguration configuration)
        {
            await base.Initialize(platibusConfiguration, configuration);

            var defaultBaseUri = new Uri(RabbitMQDefaults.BaseUri);
            platibusConfiguration.BaseUri = configuration?.GetValue<Uri>("baseUri") ?? defaultBaseUri;

            var encodingName = configuration?["encoding"] ?? RabbitMQDefaults.Encoding;
            platibusConfiguration.Encoding = string.IsNullOrWhiteSpace(encodingName)
                ? Encoding.UTF8
                : Encoding.GetEncoding(encodingName);

            platibusConfiguration.AutoAcknowledge = configuration?.GetValue("autoAcknowledge", RabbitMQDefaults.AutoAcknowledge) ?? RabbitMQDefaults.AutoAcknowledge;
            platibusConfiguration.ConcurrencyLimit = configuration?.GetValue("concurrencyLimit", RabbitMQDefaults.ConcurrencyLimit) ?? RabbitMQDefaults.ConcurrencyLimit;
            platibusConfiguration.MaxAttempts = configuration?.GetValue("maxAttempts", RabbitMQDefaults.MaxAttempts) ?? RabbitMQDefaults.MaxAttempts;

            var defaultRetryDelay = TimeSpan.Parse(RabbitMQDefaults.RetryDelay);
            platibusConfiguration.RetryDelay = configuration?.GetValue("retryDelay", defaultRetryDelay) ?? defaultRetryDelay;
            platibusConfiguration.IsDurable = configuration?.GetValue("durable", RabbitMQDefaults.Durable) ?? RabbitMQDefaults.Durable;

            var securityTokenServiceFactory = new SecurityTokenServiceFactory(platibusConfiguration.DiagnosticService);
            var securityTokensSection = configuration?.GetSection("securityTokens");
            platibusConfiguration.SecurityTokenService = await securityTokenServiceFactory.InitSecurityTokenService(securityTokensSection);
        }
    }
}
#endif