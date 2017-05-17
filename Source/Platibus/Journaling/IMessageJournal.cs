using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Journaling
{
    /// <summary>
    /// An append-only log of messages that are sent, published, and received
    /// </summary>
    public interface IMessageJournal
    {
        /// <summary>
        /// Returns the offset that represents the beginning of the message journal
        /// </summary>
        /// <param name="cancellationToken">(Optional) A cancellation token that may be used by
        /// the caller to request cancellation of the operation</param>
        /// <returns>Returns a task whose result is the offset representing the beginning of the
        /// message journal</returns>
        Task<MessageJournalOffset> GetBeginningOfJournal(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Appends a message to the end of the journal
        /// </summary>
        /// <param name="message">The message to append</param>
        /// <param name="category">The message category</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that may be used by
        /// the caller to request cancellation of the journaling operation</param>
        /// <returns>Returns a task that will complete when the journaling operation is finished</returns>
        Task Append(Message message, JournaledMessageCategory category, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Reads messages from the journal
        /// </summary>
        /// <param name="start">The absolute offset from which the read operation should begin</param>
        /// <param name="count">The maximum number of messages to be read</param>
        /// <param name="filter">(Optional) A filter that can be used to target specific messages</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that may be used by
        ///     the caller to request cancellation of the read operation</param>
        /// <returns>Returns a task whose result contains the messages that were read and a pointer
        /// to the next offset</returns>
        Task<MessageJournalReadResult> Read(MessageJournalOffset start, int count, MessageJournalFilter filter = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Reconstructs a <see cref="MessageJournalOffset"/> from its string representation
        /// </summary>
        /// <param name="str">The string representation of the offset</param>
        /// <returns>Returns the <see cref="MessageJournalOffset"/> corresponding to the specified
        /// string representation</returns>
        MessageJournalOffset ParseOffset(string str);
    }
}
