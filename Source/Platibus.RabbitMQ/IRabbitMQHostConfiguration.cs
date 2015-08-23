using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Extends <see cref="IPlatibusConfiguration"/> with configuration specific to RabbitMQ hosting
    /// </summary>
    public interface IRabbitMQHostConfiguration : IPlatibusConfiguration
    {
        /// <summary>
        /// The base URI for the RabbitMQ hosted bus instance
        /// </summary>
        /// <remarks>
        /// This is the server URI.  The use of virtual hosts is recommended.
        /// </remarks>
        Uri BaseUri { get; }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// The maxiumum number of concurrent consumers that will be started
        /// for each queue
        /// </summary>
        int ConcurrencyLimit { get; }

        /// <summary>
        /// Whether queues should be configured to automatically acknowledge
        /// messages when read by a consumer
        /// </summary>
        bool AutoAcknowledge { get; set; }

        /// <summary>
        /// The maximum number of attempts to process a message before moving it to
        /// a dead letter queue
        /// </summary>
        int MaxAttempts { get; set; }

        /// <summary>
        /// The amount of time to wait between redelivery attempts
        /// </summary>
        TimeSpan RetryDelay { get; set; }
    }
}