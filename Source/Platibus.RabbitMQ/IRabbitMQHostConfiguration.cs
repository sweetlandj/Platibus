using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public interface IRabbitMQHostConfiguration : IPlatibusConfiguration
    {
        Uri ServerUrl { get; set; }
        Encoding Encoding { get; }
    }
}