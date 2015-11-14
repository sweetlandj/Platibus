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
    /// <summary>
    /// Helper class for working with RabbitMQ
    /// </summary>
    public static class RabbitMQHelper
    {
        /// <summary>
        /// Opens a connection to a RabbitMQ server at the specified <paramref name="uri"/>
        /// </summary>
        /// <param name="uri">The RabbitMQ server URI</param>
        /// <returns>Returns a new connection to the RabbitMQ server</returns>
        public static IConnection Connect(this Uri uri)
        {            
            return new ConnectionFactory
            {
                HostName = uri.Host,
                Port = uri.Port,
                VirtualHost = uri.AbsolutePath
            }.CreateConnection();
        }

        /// <summary>
        /// Replaces characters that are not valid in queue names with underscores
        /// </summary>
        /// <param name="queueName">A queue name that may contain invalid characters</param>
        /// <returns>Returns a valid RabbitMQ queue name</returns>
        public static string ReplaceInvalidQueueNameCharacters(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException("queueName");
            return Regex.Replace(queueName, @"[^\w_\.\-\:]+", @"_");
        }

        /// <summary>
        /// Applies a naming convention to determine the name of the retry queue associated 
        /// with specified primary <paramref name="queueName"/>
        /// </summary>
        /// <param name="queueName">The primary queue name</param>
        /// <returns>Returns the name of the associated retry queue</returns>
        public static QueueName GetRetryQueueName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidQueueNameCharacters(queueName) + "-R";
        }

        /// <summary>
        /// Applies a naming convention to determine the name of the exchange for the specified
        /// primary <paramref name="queueName"/>
        /// </summary>
        /// <param name="queueName">The primary queue name</param>
        /// <returns>Returns the name of the associated exchange</returns>
        public static string GetExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidQueueNameCharacters(queueName) + "-X";
        }

        /// <summary>
        /// Applies a naming convention to determine the name of the exchange for the specified
        /// <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>Returns the name of the associated topic exchange</returns>
        public static string GetTopicExchangeName(this TopicName topic)
        {
            if (topic == null) throw new ArgumentNullException("topic");
            return "topic-" + ReplaceInvalidQueueNameCharacters(topic) + "-X";
        }

        /// <summary>
        /// Applies a naming convention to determine the name of the retry exchange associated 
        /// with the specified primary <paramref name="queueName"/>
        /// </summary>
        /// <param name="queueName">The primary queue name</param>
        /// <returns>Returns the name of the associated retry exchange</returns>
        public static string GetRetryExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidQueueNameCharacters(queueName) + "-RX";
        }

        /// <summary>
        /// Applies a naming convention to determine the name of the dead letter exchange 
        /// associated with the specified primary <paramref name="queueName"/>
        /// </summary>
        /// <param name="queueName">The primary queue name</param>
        /// <returns>Returns the name of the associated dead letter exchange</returns>
        public static string GetDeadLetterExchangeName(this QueueName queueName)
        {
            if (queueName == null) throw new ArgumentNullException("queueName");
            return ReplaceInvalidQueueNameCharacters(queueName) + "-DLX";
        }

        /// <summary>
        /// Applies a naming convention to determine the name of a subscriber queue
        /// </summary>
        /// <param name="subscriberUri">The base URI of the subscribing bus instance</param>
        /// <param name="topicName">The name of the topic</param>
        /// <returns>Returns the name of the subscription queue</returns>
        public static string GetSubscriptionQueueName(this Uri subscriberUri, TopicName topicName)
        {
            if (subscriberUri == null) throw new ArgumentNullException("subscriberUri");
            if (topicName == null) throw new ArgumentNullException("topicName");

            var hostPortVhost = ReplaceInvalidQueueNameCharacters(subscriberUri.Host);
            if (subscriberUri.Port > 0)
            {
                hostPortVhost += "_" + subscriberUri.Port;
            }

            var vhost = subscriberUri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(vhost))
            {
                hostPortVhost += "_" + ReplaceInvalidQueueNameCharacters(vhost);
            }

            return ReplaceInvalidQueueNameCharacters(topicName) + "-" + hostPortVhost;
        }

        /// <summary>
        /// Publishes a message to a queue or exchange
        /// </summary>
        /// <param name="message">The message to publish</param>
        /// <param name="principal">The sender principal</param>
        /// <param name="connection">The connection to the RabbitMQ server</param>
        /// <param name="queueName">The name of the destination queue</param>
        /// <param name="exchange">The name of the destination exchange</param>
        /// <param name="encoding">The encoding to use when converting the serialized message
        /// string to a byte stream</param>
        /// <param name="attempts">The number of attempts that have been made to publish the 
        /// message (stored in a header and used to enforce maximum delivery attempts)</param>
        /// <returns>Returns a task that will complete when the message has been published</returns>
        /// <remarks>
        /// Either a <paramref name="queueName"/> or <paramref name="exchange"/> must be specified
        /// </remarks>
        public static async Task PublishMessage(Message message, IPrincipal principal, IConnection connection,
            QueueName queueName, string exchange = "", Encoding encoding = null, int attempts = 0)
        {
            using (var channel = connection.CreateModel())
            {
                await PublishMessage(message, principal, channel, queueName, exchange, encoding, attempts);
            }
        }

        /// <summary>
        /// Publishes a message to a queue or exchange
        /// </summary>
        /// <param name="message">The message to publish</param>
        /// <param name="principal">The sender principal</param>
        /// <param name="channel">The channel to which the message should be published</param>
        /// <param name="queueName">The name of the destination queue</param>
        /// <param name="exchange">The name of the destination exchange</param>
        /// <param name="encoding">The encoding to use when converting the serialized message
        /// string to a byte stream</param>
        /// <param name="attempts">The number of attempts that have been made to publish the 
        /// message (stored in a header and used to enforce maximum delivery attempts)</param>
        /// <returns>Returns a task that will complete when the message has been published</returns>
        /// <remarks>
        /// Either a <paramref name="queueName"/> or <paramref name="exchange"/> must be specified
        /// </remarks>
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
                properties.Persistent = true;
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

        /// <summary>
        /// Reads the numbr of delivery attempts from the basic properties of a message
        /// </summary>
        /// <param name="basicProperties">The basic properties of the message</param>
        /// <returns>Returns the number of attempts that have been made to deliver the message</returns>
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

        /// <summary>
        /// Increments the total number of delivery attempts in the basic properties of a message
        /// </summary>
        /// <param name="basicProperties">The basic properties of the message</param>
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