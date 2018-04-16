using System;

namespace Platibus.Journaling
{
    /// <summary>
    /// Encapsulates data on the progress of a <see cref="MessageJournalConsumer"/>
    /// </summary>
    public class MessageJournalConsumerProgress
    {
        /// <summary>
        /// The number of entries consumed
        /// </summary>
        public long Count { get; }

        /// <summary>
        /// The timestamp of the last entry that was read
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The position of the message journal entry that was just consumed
        /// </summary>
        public MessageJournalPosition Current { get; }

        /// <summary>
        /// The position of the next entry that will be read
        /// </summary>
        public MessageJournalPosition Next { get; }

        /// <summary>
        /// Whether the entry that was just consumed is the last one in the
        /// journal as of the last read
        /// </summary>
        public bool EndOfJournal { get; }

        /// <summary>
        /// Initialies a new <see cref="MessageJournalConsumerProgress"/>
        /// </summary>
        /// <param name="count">The number of entries consumed</param>
        /// <param name="timestamp">The timestamp of the last entry that was read</param>
        /// <param name="current">The position of the message journal entry that was just consumed</param>
        /// <param name="next">The position of the next entry that will be read</param>
        /// <param name="endOfJournal">Whether the entry that was just consumed is the last one in the
        /// journal as of the last read</param>
        public MessageJournalConsumerProgress(long count, DateTime timestamp, MessageJournalPosition current,
            MessageJournalPosition next, bool endOfJournal)
        {
            Count = count;
            Timestamp = timestamp;
            Current = current ?? throw new ArgumentNullException(nameof(current));
            Next = next ?? throw new ArgumentNullException(nameof(next));
            EndOfJournal = endOfJournal;
        }
    }
}
