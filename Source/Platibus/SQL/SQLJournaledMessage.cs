using System;

namespace Platibus.SQL
{
    /// <summary>
    /// An immutable representation of a journaled message used by the 
    /// <see cref="SQLMessageJournalingService"/>
    /// </summary>
    public class SQLJournaledMessage
    {
        private readonly Message _message;
        private readonly string _category;

        /// <summary>
        /// The queued message
        /// </summary>
        public Message Message
        {
            get { return _message; }
        }

        /// <summary>
        /// The journal category, e.g. "Sent", "Received", or "Published"
        /// </summary>
        public string Category
        {
            get { return _category; }
        }

        /// <summary>
        /// Initializes a new <see cref="SQLJournaledMessage"/> with the specified
        /// <paramref name="message"/> and <paramref name="category"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="category">The journal category (e.g. "Sent", "Received", or "Published")</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/>
        /// is <c>null</c></exception>
        public SQLJournaledMessage(Message message, string category)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException("category");
            _message = message;
            _category = category;
        }
    }
}