using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Http
{
    public class OutboundQueueListener : IQueueListener
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);

        private readonly ITransportService _transportService;
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly Func<Uri, IEndpointCredentials> _credentialsFactory;

        public OutboundQueueListener(ITransportService transportService,
            IMessageJournalingService messageJournalingService, Func<Uri, IEndpointCredentials> credentialsFactory)
        {
            if (transportService == null)
            {
                throw new ArgumentNullException("transportService");
            }
            _transportService = transportService;
            _messageJournalingService = messageJournalingService;
            _credentialsFactory = credentialsFactory ??
                                  (uri => null);
        }

        public async Task MessageReceived(Message message, IQueuedMessageContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message.Headers.Expires < DateTime.UtcNow)
            {
                Log.WarnFormat("Discarding expired \"{0}\" message (ID {1}, expired {2})",
                    message.Headers.MessageName, message.Headers.MessageId, message.Headers.Expires);

                await context.Acknowledge();
                return;
            }

            var credentials = _credentialsFactory(message.Headers.Destination);

            await _transportService.SendMessage(message, credentials, cancellationToken);
            await context.Acknowledge();

            if (_messageJournalingService != null)
            {
                await _messageJournalingService.MessageSent(message, cancellationToken);
            }
        }
    }
}
