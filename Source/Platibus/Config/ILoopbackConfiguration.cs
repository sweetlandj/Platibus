using System;

namespace Platibus.Config
{
    /// <summary>
    /// An interface describing the settings particular to hosting
    /// a bus instance in a loopback configuration
    /// </summary>
    public interface ILoopbackConfiguration :  IPlatibusConfiguration
    {
        /// <summary>
        /// The base URI of the loopback bus instance
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// The message queueing service to use
        /// </summary>
        IMessageQueueingService MessageQueueingService { get; }
    }
}