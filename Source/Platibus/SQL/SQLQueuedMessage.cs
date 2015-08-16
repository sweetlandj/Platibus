using System;
using System.Security.Principal;

namespace Platibus.SQL
{
    /// <summary>
    /// An immutable representation of a queued message used by the 
    /// <see cref="SQLMessageQueueingService"/>
    /// </summary>
    public class SQLQueuedMessage
    {
        private readonly Message _message;
        private readonly IPrincipal _senderPrincipal;
        private readonly int _attempts;

        /// <summary>
        /// The queued message
        /// </summary>
        public Message Message
        {
            get { return _message; }
        }

        /// <summary>
        /// The principal that sent the message or from which the message was received
        /// </summary>
        public IPrincipal SenderPrincipal
        {
            get { return _senderPrincipal; }
        }

        /// <summary>
        /// The number of attempts that have been made so far to handle the message
        /// </summary>
        public int Attempts
        {
            get { return _attempts; }
        }

        /// <summary>
        /// Initializes a new <see cref="SQLQueuedMessage"/> with the specified
        /// <paramref name="message"/> and <paramref name="senderPrincipal"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="senderPrincipal">The principal that sent the message or
        /// from which the message was received</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/>
        /// is <c>null</c></exception>
        public SQLQueuedMessage(Message message, IPrincipal senderPrincipal)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
        }

        /// <summary>
        /// Initializes a new <see cref="SQLQueuedMessage"/> with the specified
        /// <paramref name="message"/> and <paramref name="senderPrincipal"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="senderPrincipal">The principal that sent the message or
        /// from which the message was received</param>
        /// <param name="attempts">The total number of attempts so far that have been
        /// made to handle the message</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/>
        /// is <c>null</c></exception>
        public SQLQueuedMessage(Message message, IPrincipal senderPrincipal, int attempts)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _senderPrincipal = senderPrincipal;
            _attempts = attempts;
        }
    }
}