namespace Platibus.RabbitMQ
{
    internal class DispatchResult
    {
        private readonly Message _message;
        private readonly bool _acknowledged;

        public Message Message { get { return _message; } }
        public bool Acknowledged { get { return _acknowledged; } }

        public DispatchResult(Message message, bool acknowledged)
        {
            _message = message;
            _acknowledged = acknowledged;
        }
    }
}
