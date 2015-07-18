using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHostConfigurationSection : PlatibusConfigurationSection
    {
        public const string DefaultBaseUri = "amqp://localhost:5672";

        public const string BaseUriPropertyName = "baseUri";
        public const string EncodingPropertyName = "encoding";
        public const string AutoAcknowledgePropertyName = "autoAcknowledge";
        public const string ConcurrencyLimitPropertyName = "concurrencyLimit";
        public const string MaxAttemptsPropertyName = "maxAttempts";
        public const string RetryDelayPropertyName = "retryDelay";

        [ConfigurationProperty(BaseUriPropertyName, DefaultValue = DefaultBaseUri)]
        public Uri BaseUri
        {
            get { return (Uri)base[BaseUriPropertyName]; }
            set { base[BaseUriPropertyName] = value; }
        }

        [ConfigurationProperty(EncodingPropertyName, DefaultValue = "UTF-8")]
        public string Encoding
        {
            get { return (string)base[EncodingPropertyName]; }
            set { base[EncodingPropertyName] = value; }
        }

        [ConfigurationProperty(AutoAcknowledgePropertyName, DefaultValue = false)]
        public bool AutoAcknowledge
        {
            get { return (bool)base[AutoAcknowledgePropertyName]; }
            set { base[AutoAcknowledgePropertyName] = value; }
        }

        [ConfigurationProperty(ConcurrencyLimitPropertyName, DefaultValue = 1)]
        public int ConcurrencyLimit
        {
            get { return (int)base[ConcurrencyLimitPropertyName]; }
            set { base[ConcurrencyLimitPropertyName] = value; }
        }

        [ConfigurationProperty(MaxAttemptsPropertyName, DefaultValue = 10)]
        public int MaxAttempts
        {
            get { return (int)base[MaxAttemptsPropertyName]; }
            set { base[MaxAttemptsPropertyName] = value; }
        }

        [ConfigurationProperty(RetryDelayPropertyName, DefaultValue = 10)]
        public TimeSpan RetryDelay
        {
            get { return (TimeSpan)base[RetryDelayPropertyName]; }
            set { base[RetryDelayPropertyName] = value; }
        }
    }
}
