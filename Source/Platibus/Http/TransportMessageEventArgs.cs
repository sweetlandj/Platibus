using System;
using System.Security.Principal;
using System.Threading;

namespace Platibus.Http
{
    /// <inheritdoc />
    /// <summary>
    /// Arguments relating to the receipt of a <see cref="Message"/>
    /// by a transport service
    /// </summary>
    public class TransportMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The message that was received
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// The user that sent the message
        /// </summary>
        public IPrincipal Principal { get; }

        /// <summary>
        /// A cancellation token that can be signaled by the sender to
        /// request cancelation of message handling
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Http.TransportMessageReceivedEventArgs" />
        /// </summary>
        /// <param name="message">The message that was received</param>
        /// <param name="principal">The user that sent the message</param>
        /// <param name="cancellationToken">A cancellation token that can be signaled by 
        /// the sender to request cancelation of message handling</param>
        public TransportMessageEventArgs(Message message, IPrincipal principal, CancellationToken cancellationToken)
        {
            Message = message;
            Principal = principal;
            CancellationToken = cancellationToken;
        }
    }
}
