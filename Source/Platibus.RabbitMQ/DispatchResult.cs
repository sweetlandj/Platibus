namespace Platibus.RabbitMQ
{
    internal class DispatchResult
    {
        public Message Message { get; }

        public bool Acknowledged { get; }

        public DispatchResult(Message message, bool acknowledged)
        {
            Message = message;
            Acknowledged = acknowledged;
        }
    }
}
