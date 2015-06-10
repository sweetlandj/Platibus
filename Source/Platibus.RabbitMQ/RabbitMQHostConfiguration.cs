using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHostConfiguration : PlatibusConfiguration, IRabbitMQHostConfiguration
    {
        public Uri ServerUrl { get; set; }
        public Encoding Encoding { get; set; }
    }
}
