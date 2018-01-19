using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// Thrown to indicate an error occurred while encrypting or decrypting a message
    /// </summary>
    public class MessageEncryptionException : AggregateException
    {
        /// <inheritdoc />
        public MessageEncryptionException()
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }

        /// <inheritdoc />
        protected MessageEncryptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        public MessageEncryptionException(string message, params Exception[] innerExceptions) : base(message, innerExceptions)
        {
        }
    }
}
