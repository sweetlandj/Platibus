using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHostConfigurationSection : PlatibusConfigurationSection
    {
        public const string DefaultServerUrl = "amqp://localhost:5672";

        public const string ServerUrlPropertyName = "serverUrl";
        public const string EncodingPropertyName = "encoding";

        [ConfigurationProperty(ServerUrlPropertyName, DefaultValue = DefaultServerUrl)]
        public Uri ServerUrl
        {
            get { return (Uri)base[ServerUrlPropertyName]; }
            set { base[ServerUrlPropertyName] = value; }
        }

        [ConfigurationProperty(EncodingPropertyName, DefaultValue = "UTF-8")]
        public string Encoding
        {
            get { return (string)base[EncodingPropertyName]; }
            set { base[EncodingPropertyName] = value; }
        }
    }
}
