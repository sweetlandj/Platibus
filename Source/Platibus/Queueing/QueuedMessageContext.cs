using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Queueing
{
    internal class QueuedMessageContext : IQueuedMessageContext
    {
        private readonly Message _message;
        private readonly IPrincipal _senderPrincipal;

        public QueuedMessageContext(Message message, IPrincipal senderPrincipal)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
        }

        public bool Acknowledged { get; private set; }

        public IMessageHeaders Headers
        {
            get { return _message.Headers; }
        }

        public IPrincipal Principal
        {
            get { return _senderPrincipal; }
        }

        public Task Acknowledge()
        {
            Acknowledged = true;
            return Task.FromResult(true);
        }
    }
}
