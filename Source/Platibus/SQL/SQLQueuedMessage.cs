using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    class SQLQueuedMessage
    {
        private readonly Message _message;
        private readonly IPrincipal _senderPrincipal;

        public Message Message { get { return _message; } }
        public IPrincipal SenderPrincipal { get { return _senderPrincipal; } }

        public SQLQueuedMessage(Message message, IPrincipal senderPrincipal)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
        }
    }
}
