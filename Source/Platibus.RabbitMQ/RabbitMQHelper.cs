using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Platibus.Security;
using RabbitMQ.Client;

namespace Platibus.RabbitMQ
{
    public static class RabbitMQHelper
    {
        public static IConnection Connect(this Uri uri)
        {            
            return new ConnectionFactory
            {
                HostName = uri.Host,
                Port = uri.Port,
                VirtualHost = uri.AbsolutePath
            }.CreateConnection();
        }

        public static string ReplaceInvalidChars(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentNullException("str");
            return Regex.Replace(str, @"[^\w_\.\-\:]+", @"_");
        }

        public static QueueName GetRetryQueueName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidChars(queueName) + "-R";
        }

        public static string GetExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidChars(queueName) + "-X";
        }

        public static string GetTopicExchangeName(this TopicName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return "topic-" + ReplaceInvalidChars(queueName) + "-X";
        }

        public static string GetRetryExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidChars(queueName) + "-RX";
        }

        public static string GetDeadLetterExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidChars(queueName) + "-DLX";
        }

        public static string GetSubscriptionQueueName(this Uri subscriberUri, TopicName topicName)
        {
            if (subscriberUri == null) throw new ArgumentNullException("subscriberUri");
            if (topicName == null) throw new ArgumentNullException("topicName");

            var hostPortVhost = ReplaceInvalidChars(subscriberUri.Host);
            if (subscriberUri.Port > 0)
            {
                hostPortVhost += "_" + subscriberUri.Port;
            }

            var vhost = subscriberUri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(vhost))
            {
                hostPortVhost += "_" + ReplaceInvalidChars(vhost);
            }

            return ReplaceInvalidChars(topicName) + "-" + hostPortVhost;
        }

        public static async Task PublishMessage(Message message, IPrincipal principal, IConnection connection,
            QueueName queueName, string exchange = "", Encoding encoding = null, int attempts = 0)
        {
            using (var channel = connection.CreateModel())
            {
                await PublishMessage(message, principal, channel, queueName, exchange, encoding, attempts);
            }
        }

        public static async Task PublishMessage(Message message, IPrincipal principal, IModel channel,
            QueueName queueName, string exchange = "", Encoding encoding = null, int attempts = 0)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (channel == null) throw new ArgumentNullException("channel");

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var senderPrincipal = principal as SenderPrincipal;
            if (senderPrincipal == null && principal != null)
            {
                senderPrincipal = new SenderPrincipal(principal);
            }

            using (var stringWriter = new StringWriter())
            using (var messageWriter = new MessageWriter(stringWriter))
            {
                await messageWriter.WritePrincipal(senderPrincipal);
                await messageWriter.WriteMessage(message);
                var messageBody = stringWriter.ToString();
                var properties = channel.CreateBasicProperties();
                properties.SetPersistent(true);
                properties.ContentEncoding = Encoding.UTF8.HeaderName;
                properties.ContentType = message.Headers.ContentType;
                properties.MessageId = message.Headers.MessageId.ToString();
                properties.CorrelationId = message.Headers.RelatedTo.ToString();
                var headers = properties.Headers;
                if (headers == null)
                {
                    headers = new Dictionary<string, object>();
                    properties.Headers = headers;
                }
                headers["x-platibus-attempts"] = attempts;
                var body = encoding.GetBytes(messageBody);

                var ex = exchange ?? "";
                var q = (string) queueName ?? "";

                channel.BasicPublish(ex, q, properties, body);
            }
        }

        public static int GetDeliveryAttempts(this IBasicProperties basicProperties)
        {
            var headers = basicProperties.Headers;
            if (headers == null) return 0;

            var attempts = headers["x-platibus-attempts"];
            if (attempts == null) return 0;
            if (attempts is int) return (int) attempts;

            var attemptStr = attempts.ToString();
            int parsedInt;
            if (int.TryParse(attemptStr, out parsedInt))
            {
                return parsedInt;
            }
            return 0;
        }

        public static void IncrementDeliveryAttempts(this IBasicProperties basicProperties)
        {
            var attempts = basicProperties.GetDeliveryAttempts();
            attempts++;
            var headers = basicProperties.Headers;
            if (headers == null)
            {
                headers = new Dictionary<string, object>();
                basicProperties.Headers = headers;
            }
            headers["x-platibus-attempts"] = attempts;
        }
    }
}