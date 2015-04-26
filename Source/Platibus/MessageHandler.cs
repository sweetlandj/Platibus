using Common.Logging;
using Platibus.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    static class MessageHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Core);

        public static async Task HandleMessage(IMessageNamingService messageNamingService, ISerializationService serializationService, IEnumerable<IMessageHandler> messageHandlers, Message message, IMessageContext messageContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.Headers.Expires < DateTime.UtcNow)
            {
                Log.WarnFormat("Discarding expired \"{0}\" message (ID {1}, expired {2})", message.Headers.MessageName,
                    message.Headers.MessageId, message.Headers.Expires);

                messageContext.Acknowledge();
                return;
            }

            var messageType = messageNamingService.GetTypeForName(message.Headers.MessageName);
            var serializer = serializationService.GetSerializer(message.Headers.ContentType);
            var messageContent = serializer.Deserialize(message.Content, messageType);
            var handlingTasks = messageHandlers.Select(handler => handler.HandleMessage(messageContent, messageContext, cancellationToken));

            await Task.WhenAll(handlingTasks).ConfigureAwait(false);
        }
    }
}
