using Platibus.Diagnostics;
using Platibus.Journaling;
using System;
using System.Collections.Generic;
using System.Text;
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Options governing the behavior of the <see cref="RabbitMQTransportService"/>
    /// </summary>
    public class RabbitMQTransportServiceOptions
    {
        /// <summary>
        /// The RMQP URL of this instance (including VHost path)
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// The connection manager used to obtain connections to RabbitMQ server(s)
        /// </summary>
        public IConnectionManager ConnectionManager { get; }

        /// <summary>
        /// The diagnostic service through which diagnostic events related to RabbitMQ will be raised
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }

        /// <summary>
        /// The message journal used to record copies of messages that are sent, received, or published
        /// </summary>
        public IMessageJournal MessageJournal { get; set; }

        /// <summary>
        /// A service that issues and validates security tokens to capture principal claims
        /// with queued messages
        /// </summary>
        public ISecurityTokenService SecurityTokenService { get; set; }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Default options to apply to new message queues
        /// </summary>
        public QueueOptions DefaultQueueOptions { get; set; }

        /// <summary>
        /// The topics that will be published to
        /// </summary>
        public IEnumerable<TopicName> Topics { get; set; }

        /// <summary>
        /// Initializes new <see cref="RabbitMQMessageQueueingOptions"/>
        /// </summary>
        /// <param name="baseUri">The RMQP URL of this instance (including VHost path)</param>
        /// <param name="connectionManager">The connection manager used to obtain connections to 
        /// RabbitMQ server(s)</param>
        public RabbitMQTransportServiceOptions(Uri baseUri, IConnectionManager connectionManager)
        {
            BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            ConnectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }
    }
}
