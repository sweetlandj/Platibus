using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Extends <see cref="PlatibusConfiguration"/> with configuration specific to RabbitMQ hosting
    /// </summary>
    public class RabbitMQHostConfiguration : PlatibusConfiguration, IRabbitMQHostConfiguration
    {
        /// <summary>
        /// The base URI for the RabbitMQ hosted bus instance
        /// </summary>
        /// <remarks>
        /// This is the server URI.  The use of virtual hosts is recommended.
        /// </remarks>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// The maxiumum number of concurrent consumers that will be started
        /// for each queue
        /// </summary>
        public int ConcurrencyLimit { get; set; }

        /// <summary>
        /// Whether queues should be configured to automatically acknowledge
        /// messages when read by a consumer
        /// </summary>
        public bool AutoAcknowledge { get; set; }

        /// <summary>
        /// The maximum number of attempts to process a message before moving it to
        /// a dead letter queue
        /// </summary>
        public int MaxAttempts { get; set; }

        /// <summary>
        /// The amount of time to wait between redelivery attempts
        /// </summary>
        public TimeSpan RetryDelay { get; set; }
    }
}
