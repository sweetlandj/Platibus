
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHostConfigurationManager
    {
        public static async Task<RabbitMQHostConfiguration> LoadConfiguration(string sectionName = "platibus.rabbitmq")
        {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException("sectionName");
            var configSection = (RabbitMQHostConfigurationSection)ConfigurationManager.GetSection(sectionName) ??
                                new RabbitMQHostConfigurationSection();

            var configuration = await PlatibusConfigurationManager.LoadConfiguration<RabbitMQHostConfiguration>(sectionName);

            configuration.BaseUri = configSection.BaseUri 
                ?? new Uri(RabbitMQHostConfigurationSection.DefaultBaseUri);

            configuration.Encoding = string.IsNullOrWhiteSpace(configSection.Encoding) 
                ? Encoding.UTF8 
                : Encoding.GetEncoding(configSection.Encoding);

            configuration.AutoAcknowledge = configSection.AutoAcknowledge;
            configuration.ConcurrencyLimit = configSection.ConcurrencyLimit;
            configuration.MaxAttempts = configuration.MaxAttempts;
            configuration.RetryDelay = configuration.RetryDelay;
            
            return configuration;
        }
    }
}
