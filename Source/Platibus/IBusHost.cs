
using System;

namespace Platibus
{
    /// <summary>
    /// An interface describing an object that hosts a <see cref="IBus"/> instance
    /// and handles transport concerns.
    /// </summary>
    public interface IBusHost
    {
        /// <summary>
        /// Specifies the base URI at which the host is listening.
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// Returns the transport service provided by the host for sending
        /// messages and subscribing to publications.
        /// </summary>
        ITransportService TransportService { get; }
    }
}
