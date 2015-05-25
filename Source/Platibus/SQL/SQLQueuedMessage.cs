using System;
using System.Security.Principal;

namespace Platibus.SQL
{
    public class SQLQueuedMessage
    {
        private readonly Message _message;
        private readonly IPrincipal _senderPrincipal;
        private readonly int _attempts;

        public Message Message
        {
            get { return _message; }
        }

        public IPrincipal SenderPrincipal
        {
            get { return _senderPrincipal; }
        }

        public int Attempts
        {
            get { return _attempts; }
        }

        public SQLQueuedMessage(Message message, IPrincipal senderPrincipal)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
        }

        public SQLQueuedMessage(Message message, IPrincipal senderPrincipal, int attempts)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
            _attempts = attempts;
        }
    }
}