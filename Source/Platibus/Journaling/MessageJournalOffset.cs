namespace Platibus.Journaling
{
    /// <summary>
    /// An representation of an absolute position within a message journal used for polling and
    /// paging read operations
    /// </summary>
    public abstract class MessageJournalOffset
    {
        /// <summary>
        /// Returns a string representation of the message journal offset suitable for
        /// network transport to consumers
        /// </summary>
        /// <returns>Returns a string representation of the message journal offset</returns>
        public abstract override string ToString();
    }
}
