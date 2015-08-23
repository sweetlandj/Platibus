
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Factory class used to initialize <see cref="RabbitMQHostConfiguration"/> objects from
    /// declarative configuration elements in application configuration files.
    /// </summary>
    public class RabbitMQHostConfigurationManager
    {
        /// <summary>
        /// Initializes and returns a <see cref="RabbitMQHostConfiguration"/> instance based on
        /// the <see cref="RabbitMQHostConfigurationSection"/> with the specified 
        /// <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">(Optional) The name of the configuration section 
        /// (default is "platibus.rabbitmq")</param>
        /// <param name="processConfigurationHooks">(Optional) Whether to initialize and
        /// process implementations of <see cref="IConfigurationHook"/> found in the
        /// application domain (default is true)</param>
        /// <returns>Returns a task whose result will be an initialized 
        /// <see cref="PlatibusConfiguration"/> object</returns>
        /// <seealso cref="PlatibusConfigurationSection"/>
        /// <seealso cref="IConfigurationHook"/>
        public static async Task<RabbitMQHostConfiguration> LoadConfiguration(string sectionName = "platibus.rabbitmq", 
            bool processConfigurationHooks = true)
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
