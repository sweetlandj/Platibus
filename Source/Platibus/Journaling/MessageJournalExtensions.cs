using System;

namespace Platibus.Journaling
{
    /// <summary>
    /// Extension methods for working with message journals
    /// </summary>
    public static class MessageJournalExtensions
    {
        /// <summary>
        /// Determines the timestamp for the journaled message based on the journal category and
        /// correlating dates in the message headers.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <remarks>
        /// Falls back to the current UTC date/time if the expected headers are not set
        /// </remarks>
        public static DateTime GetJournalTimestamp(this Message message, MessageJournalCategory category)
        {
            var headers = message.Headers;
            var timestamp = DateTime.UtcNow;
            if (Equals(category, MessageJournalCategory.Sent) && headers.Sent != default(DateTime))
            {
                timestamp = headers.Sent;
            }
            else if (Equals(category, MessageJournalCategory.Received) && headers.Received != default(DateTime))
            {
                timestamp = headers.Received;
            }
            else if (Equals(category, MessageJournalCategory.Received) && headers.Published != default(DateTime))
            {
                timestamp = headers.Published;
            }
            return timestamp;
        }
    }
}
