using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public class RabbitMQHostConfiguration : PlatibusConfiguration, IRabbitMQHostConfiguration
    {
        public Uri BaseUri { get; set; }
        public Uri RabbitMQServerUrl { get; set; }
        public Encoding Encoding { get; set; }
        public int ConcurrencyLimit { get; set; }
        public bool AutoAcknowledge { get; set; }
        public int MaxAttempts { get; set; }
        public TimeSpan RetryDelay { get; set; }
    }
}
