namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Default values for RabbitMQ
    /// </summary>
    public static class RabbitMQDefaults
    {
        /// <summary>
        /// The default base URI
        /// </summary>
        public const string BaseUri = "amqp://localhost:5672";

        /// <summary>
        /// The default message encoding
        /// </summary>
        public const string Encoding = "UTF-8";

        /// <summary>
        /// The number of messages that can be concurrently processed 
        /// from each queue 
        /// </summary>
        public const int ConcurrencyLimit = 1;

        /// <summary>
        /// The maximum number of attempts made to process each message
        /// </summary>
        public const int MaxAttempts = 10;

        /// <summary>
        /// The delay between message processing attempts
        /// </summary>
        public const string RetryDelay = "00:00:05";

        /// <summary>
        /// Whether messages should be automatically acknowledged
        /// </summary>
        public const bool AutoAcknowledge = false;

        /// <summary>
        /// Whether queues should be durable
        /// </summary>
        public const bool Durable = true;
    }
}
