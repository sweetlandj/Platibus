using System;
using System.Runtime.Serialization;

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
        /// <summary>
        /// Initializes a new <see cref="MessageNotAcknowledgedException"/>
        /// </summary>
        public MessageNotAcknowledgedException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MessageNotAcknowledgedException"/>
        /// with the specified detail message
        /// </summary>
        /// <param name="message">The detail message</param>
        public MessageNotAcknowledgedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MessageNotAcknowledgedException"/>
        /// with the specified detail message and nested exception
        /// </summary>
        /// <param name="message">The detail message</param>
        /// <param name="innerException">The nested exception</param>
        public MessageNotAcknowledgedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a serialized <see cref="MessageNotAcknowledgedException"/>
        /// from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public MessageNotAcknowledgedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}