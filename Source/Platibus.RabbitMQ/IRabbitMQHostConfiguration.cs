using System;
using System.Text;
using Platibus.Config;

namespace Platibus.RabbitMQ
{
    public interface IRabbitMQHostConfiguration : IPlatibusConfiguration
    {
        Uri BaseUri { get; }
        Encoding Encoding { get; }
        int ConcurrencyLimit { get; }
        bool AutoAcknowledge { get; set; }
        int MaxAttempts { get; set; }
        TimeSpan RetryDelay { get; set; }
    }
}