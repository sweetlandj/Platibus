using System;
using System.Text;
using System.Threading.Tasks;
using Platibus.Config;
using Platibus.Config.Extensibility;

namespace Platibus.RabbitMQ
{
    [Provider("RabbitMQ")]
    public class RabbitMQServicesProvider : IMessageQueueingServiceProvider
    {
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            var uri = configuration.GetUri("uri") ?? new Uri("amqp://localhost:5672");

            var encodingName = configuration.GetString("encoding");
            if (string.IsNullOrWhiteSpace(encodingName))
            {
                encodingName = "UTF-8";
            }

            var encoding = ParseEncoding(encodingName);
            var messageQueueingService = new RabbitMQMessageQueueingService(uri, encoding);
            messageQueueingService.Init();
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