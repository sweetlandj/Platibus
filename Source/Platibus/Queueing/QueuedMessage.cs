using System;
using System.Security.Principal;

namespace Platibus.Queueing
{
    /// <summary>
    /// An immutable representation of a queued message
    /// </summary>
    public class QueuedMessage
    {
        private readonly Message _message;
        private readonly IPrincipal _principal;
        private readonly int _attempts;

        /// <summary>
        /// The queued message
        /// </summary>
        public Message Message
        {
            get { return _message; }
        }

        /// <summary>
        /// The principal represending the message sender
        /// </summary>
        public IPrincipal Principal
        {
            get { return _principal; }
        }

        /// <summary>
        /// The number of attempts that have been made so far to handle the message
        /// </summary>
        public int Attempts { get { return _attempts; } }

        /// <summary>
        /// Initializes a new <see cref="QueuedMessage"/>
        /// </summary>
        /// <param name="message">The queued message</param>
        /// <param name="principal">(Optional) The principal representing the message sender</param>
        /// <param name="attempts">The number of attempts that have been made to process the
        ///     queued message</param>
        public QueuedMessage(Message message, IPrincipal principal, int attempts)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
            _attempts = attempts;
            _principal = principal;
        }
    }
}
