using Platibus.Diagnostics;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Factory for creating new diagnostic <see cref="RabbitMQEvent"/>s
    /// </summary>
    public class RabbitMQEventBuilder : DiagnosticEventBuilder
    {
        /// <summary>
        /// The exchange to which the event pertains
        /// </summary>
        public string Exchange { get; set; }

        /// <summary>
        /// The channel number to which the event pertains, if applicable
        /// </summary>
        public int? ChannelNumber { get; set; }

        /// <summary>
        /// The consumer tag to which the event pertains, if applicable
        /// </summary>
        public string ConsumerTag { get; set; }

        /// <summary>
        /// The delivery tag to which the event pertains, if applicable
        /// </summary>
        public ulong? DeliveryTag { get; set; }

        /// <summary>
        /// Initializes a new <see cref="RabbitMQEventBuilder"/> for the specified 
        /// </summary>
        /// <param name="source">The object that will produce the event</param>
        /// <param name="type">The type of event</param>
        public RabbitMQEventBuilder(object source, DiagnosticEventType type) : base(source, type)
        {
        }

        /// <inheritdoc />
        public override DiagnosticEvent Build()
        {
            return new RabbitMQEvent(Source, Type, Detail, Exception, Message, Endpoint, Queue, Topic, Exchange, ChannelNumber, ConsumerTag, DeliveryTag);
        }
    }
}
