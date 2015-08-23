using System;
using System.Text;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Provider for services based on RabbitMQ
    /// </summary>
    [Provider("RabbitMQ")]
    public class RabbitMQServicesProvider : IMessageQueueingServiceProvider
    {
        /// <summary>
        /// Creates an initializes a <see cref="IMessageQueueingService"/>
        /// based on the provideds <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The journaling configuration
        /// element</param>
        /// <returns>Returns a task whose result is an initialized
        /// <see cref="IMessageQueueingService"/></returns>
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            var uri = configuration.GetUri("uri") ?? new Uri("amqp://localhost:5672");

            var encodingName = configuration.GetString("encoding");
            if (string.IsNullOrWhiteSpace(encodingName))
            {
                encodingName = "UTF-8";
            }

            var encoding = ParseEncoding(encodingName);
            var messageQueueingService = new RabbitMQMessageQueueingService(uri, encoding: encoding);
            return Task.FromResult<IMessageQueueingService>(messageQueueingService);
        }

        private static Encoding ParseEncoding(string encodingName)
        {
            switch (encodingName.ToUpper())
            {
                case "UTF7":
                case "UTF-7":
                    return Encoding.UTF7;
                case "UTF8":
                case "UTF-8":
                    return Encoding.UTF8;
                case "UTF32":
                case "UTF-32":
                    return Encoding.UTF32;
                case "UNICODE":
                    return Encoding.Unicode;
                case "ASCII":
                    return Encoding.ASCII;
                default:
                    return Encoding.UTF8;
            }
        }
    }
}