using System;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// An interface describing an object that manages RabbitMQ connections
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Returns a RabbitMQ connection to the server at the specified
        /// <paramref name="uri"/>
        /// </summary>
        /// <param name="uri">The URI of the RabbitMQ server</param>
        /// <returns>Returns a connection to the specified RabbitMQ server</returns>
        IConnection GetConnection(Uri uri);
    }
}