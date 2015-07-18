using System;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    public interface IConnectionManager
    {
        IConnection GetConnection(Uri uri);
    }
}