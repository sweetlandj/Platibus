using System;
using System.Configuration;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Extends the <see cref="PlatibusConfigurationSection"/> with configuration specific
    /// to RabbitMQ
    /// </summary>
    public class RabbitMQHostConfigurationSection : PlatibusConfigurationSection
    {
        /// <summary>
        /// The default base URI
        /// </summary>
        public const string DefaultBaseUri = "amqp://localhost:5672";

        private const string BaseUriPropertyName = "baseUri";
        private const string EncodingPropertyName = "encoding";
        private const string AutoAcknowledgePropertyName = "autoAcknowledge";
        private const string ConcurrencyLimitPropertyName = "concurrencyLimit";
        private const string MaxAttemptsPropertyName = "maxAttempts";
        private const string RetryDelayPropertyName = "retryDelay";

        /// <summary>
        /// The base URI for the RabbitMQ hosted bus instance
        /// </summary>
        /// <remarks>
        /// This is the server URI.  The use of virtual hosts is recommended.
        /// </remarks>
        [ConfigurationProperty(BaseUriPropertyName, DefaultValue = DefaultBaseUri)]
        public Uri BaseUri
        {
            get { return (Uri)base[BaseUriPropertyName]; }
            set { base[BaseUriPropertyName] = value; }
        }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        [ConfigurationProperty(EncodingPropertyName, DefaultValue = "UTF-8")]
        public string Encoding
        {
            get { return (string)base[EncodingPropertyName]; }
            set { base[EncodingPropertyName] = value; }
        }

        /// <summary>
        /// Whether queues should be configured to automatically acknowledge
        /// messages when read by a consumer
        /// </summary>
        [ConfigurationProperty(AutoAcknowledgePropertyName, DefaultValue = false)]
        public bool AutoAcknowledge
        {
            get { return (bool)base[AutoAcknowledgePropertyName]; }
            set { base[AutoAcknowledgePropertyName] = value; }
        }

        /// <summary>
        /// The maxiumum number of concurrent consumers that will be started
        /// for each queue
        /// </summary>
        [ConfigurationProperty(ConcurrencyLimitPropertyName, DefaultValue = 1)]
        public int ConcurrencyLimit
        {
            get { return (int)base[ConcurrencyLimitPropertyName]; }
            set { base[ConcurrencyLimitPropertyName] = value; }
        }

        /// <summary>
        /// The maximum number of attempts to process a message before moving it to
        /// a dead letter queue
        /// </summary>
        [ConfigurationProperty(MaxAttemptsPropertyName, DefaultValue = 10)]
        public int MaxAttempts
        {
            get { return (int)base[MaxAttemptsPropertyName]; }
            set { base[MaxAttemptsPropertyName] = value; }
        }

        /// <summary>
        /// The amount of time to wait between redelivery attempts
        /// </summary>
        [ConfigurationProperty(RetryDelayPropertyName, DefaultValue = "00:00:05")]
        public TimeSpan RetryDelay
        {
            get { return (TimeSpan)base[RetryDelayPropertyName]; }
            set { base[RetryDelayPropertyName] = value; }
        }
    }
}
