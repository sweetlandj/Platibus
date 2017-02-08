using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Config;

namespace Platibus
{
    /// <summary>
    /// An <see cref="IMessageJournalingService"/> decorator that allows or inhibits
    /// message journaling operations based on user configuration.
    /// </summary>
    /// <see cref="JournalingElement.JournalSentMessages"/>
    /// <see cref="JournalingElement.JournalReceivedMessages"/>
    /// <see cref="JournalingElement.JournalPublishedMessages"/>
    public class FilteredMessageJournalingService : IMessageJournalingService
    {
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly bool _journalSentMessages;
        private readonly bool _journalReceivedMessages;
        private readonly bool _journalPublishedMessages;

        /// <summary>
        /// Initializes a new <see cref="FilteredMessageJournalingService"/> instance
        /// </summary>
        /// <param name="messageJournalingService">The decorated message journaling service</param>
        /// <param name="journalSentMessages">Whether to allow sent messages to be journaled</param>
        /// <param name="journalReceivedMessages">Whether to allow received messages to be journaled</param>
        /// <param name="journalPublishedMessages">Whether to allow published messages to be journaled</param>
        public FilteredMessageJournalingService(IMessageJournalingService messageJournalingService, bool journalSentMessages, bool journalReceivedMessages, bool journalPublishedMessages)
        {
            if (messageJournalingService == null) throw new ArgumentNullException("messageJournalingService");

            _messageJournalingService = messageJournalingService;
            _journalSentMessages = journalSentMessages;
            _journalReceivedMessages = journalReceivedMessages;
            _journalPublishedMessages = journalPublishedMessages;
        }

        /// <inheritdoc />
        public async Task MessageSent(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_journalSentMessages)
            {
                await _messageJournalingService.MessageSent(message, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task MessageReceived(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_journalReceivedMessages)
            {
                await _messageJournalingService.MessageReceived(message, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task MessagePublished(Message message, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_journalPublishedMessages)
            {
                await _messageJournalingService.MessagePublished(message, cancellationToken);
            }
        }
    }
}
