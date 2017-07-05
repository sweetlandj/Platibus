using Platibus.Diagnostics;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// <see cref="DiagnosticEventType"/>s specific to RabbitMQ
    /// </summary>
    public static class RabbitMQEventType
    {
        /// <summary>
        /// Emitted whenever a RabbitMQ connection is opened
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConnectionOpened = new DiagnosticEventType("RabbitMQConnectionOpened", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever a RabbitMQ connection fails and must be reconnected
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQReconnect = new DiagnosticEventType("RabbitMQReconnect", DiagnosticEventLevel.Warn);
        
        /// <summary>
        /// Emitted whenever a RabbitMQ connection is closed
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConnectionClosed = new DiagnosticEventType("RabbitMQConnectionClosed", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever there is an error opening or closing a RabbitMQ connection
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConnectionError = new DiagnosticEventType("RabbitMQConnectionError", DiagnosticEventLevel.Error);


        /// <summary>
        /// Emitted whenever a RabbitMQ connection is aborted
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConnectionAborted = new DiagnosticEventType("RabbitMQConnectionAborted", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever a channel is created 
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQChannelCreated = new DiagnosticEventType("RabbitMQChannelCreated", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever a channel cannot be created
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQChannelCreationFailed = new DiagnosticEventType("RabbitMQChannelCreationFailed", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever a channel is created 
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQChannelClosed = new DiagnosticEventType("RabbitMQChannelClosed", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted when closure of a channel fails
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQChannelCloseError = new DiagnosticEventType("RabbitMQChannelCloseError", DiagnosticEventLevel.Warn);
        
        /// <summary>
        /// Emitted whenever a consumer is added to a channel
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConsumerAdded = new DiagnosticEventType("RabbitMQConsumerAdded", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever a consumer is canceled
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConsumerCanceled = new DiagnosticEventType("RabbitMQConsumerCanceled", DiagnosticEventLevel.Info);
        
        /// <summary>
        /// Emitted when cancelation of a consumer fails
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQConsumerCancelError = new DiagnosticEventType("RabbitMQConsumerCancelError", DiagnosticEventLevel.Warn);

        /// <summary>
        /// Emitted whenever the RabbitMQ host is started
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQHostStarted = new DiagnosticEventType("RabbitMQHostStarted", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever the RabbitMQ host is stoppped
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQHostStopped = new DiagnosticEventType("RabbitMQHostStopped", DiagnosticEventLevel.Info);

        /// <summary>
        /// Emitted whenever there is an error handling deliver of a message from a RabbitMQ queue
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQDeliveryError = new DiagnosticEventType("RabbitMQDeliveryError", DiagnosticEventLevel.Error);

        /// <summary>
        /// Emitted whenever an exchange is declared
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQExchangeDeclared = new DiagnosticEventType("RabbitMQExchangeDeclared", DiagnosticEventLevel.Debug);

        /// <summary>
        /// Emitted whenever an queue is declared
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQQueueDeclared = new DiagnosticEventType("RabbitMQExchangeDeclared", DiagnosticEventLevel.Debug);

        /// <summary>
        /// Emitted whenever an queue is bound to an exchange
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQQueueBound = new DiagnosticEventType("RabbitMQQueueBound", DiagnosticEventLevel.Debug);

        /// <summary>
        /// Emitted whenever there is an error binding a queue to an exchange
        /// </summary>
        public static readonly DiagnosticEventType RabbitMQQueueBindError = new DiagnosticEventType("RabbitMQQueueBindError", DiagnosticEventLevel.Error);
    }
}
