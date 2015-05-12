using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.RabbitMQ
{
    class RabbitMQQueuedMessageContext : IQueuedMessageContext
    {
        private readonly IMessageHeaders _headers;
        private readonly IPrincipal _senderPrincipal;
        
        public IMessageHeaders Headers
        {
            get { return _headers; }
        }

        public IPrincipal SenderPrincipal
        {
            get { return _senderPrincipal; }
        }

        public bool Acknowledged { get; private set; }

        public RabbitMQQueuedMessageContext(IMessageHeaders headers, IPrincipal senderPrincipal)
        {
            _headers = headers;
            _senderPrincipal = senderPrincipal;
        }

        public Task Acknowledge()
        {
            Acknowledged = true;
            return Task.FromResult(true);
        }
    }
}
