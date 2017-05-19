namespace Platibus.Journaling
{
    /// <summary>
    /// An representation of an absolute position within a message journal used for polling and
    /// paging read operations
    /// </summary>
    public abstract class MessageJournalPosition
    {
        /// <summary>
        /// Returns a string representation of the message journal offset suitable for
        /// network transport to consumers
        /// </summary>
        /// <returns>Returns a string representation of the message journal offset</returns>
        public abstract override string ToString();

        /// <summary>
        /// Determins whether this message journal offset is equal to another object
        /// </summary>
        /// <param name="obj">An object</param>
        /// <returns>Returns <c>true</c> if the specified object is equal to this message journal
        /// offset; <c>false</c> otherwise</returns>
        public abstract override bool Equals(object obj);

        /// <inheritdoc />
        public abstract override int GetHashCode();
    }
}
