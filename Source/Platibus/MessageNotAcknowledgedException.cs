using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Platibus
{
    /// <summary>
    /// Thrown to indicate that a message was not acknowledged by any of the
    /// handlers that are configured to process it.  This is used primarily
    /// as a means to communicate failure to process the message to the
    /// transport during message acceptance so that the transport can respond
    /// appropriately to the sender.
    /// </summary>
    [Serializable]
    public class MessageNotAcknowledgedException : ApplicationException
    {
        public MessageNotAcknowledgedException()
        {
        }

        public MessageNotAcknowledgedException(string message)
            : base(message)
        {
        }

        public MessageNotAcknowledgedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MessageNotAcknowledgedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
