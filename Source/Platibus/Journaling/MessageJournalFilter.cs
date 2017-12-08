using System;
using System.Collections.Generic;

namespace Platibus.Journaling
{
    /// <summary>
    /// Criteria that can be specified by the caller to constrain the results of a journal read
    /// </summary>
    public class MessageJournalFilter
    {
        private IList<MessageJournalCategory> _categories;
        private IList<TopicName> _topics;

        /// <summary>
        /// Specifies the categories to which results should be constrained
        /// </summary>
        public IList<MessageJournalCategory> Categories
        {
            get => _categories ?? (_categories = new List<MessageJournalCategory>());
            set => _categories = value;
        }

        /// <summary>
        /// Specifies the topics to which results should be constrained
        /// </summary>
        public IList<TopicName> Topics
        {
            get => _topics ?? (_topics = new List<TopicName>());
            set => _topics = value;
        }

        /// <summary>
        /// Specifies the lower bound of the date/time range to which results should be constrained
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Specifies the upper bound of the date/time range to which results should be constrained
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Specifies the base URI of the originating instance to which results should be 
        /// constrained
        /// </summary>
        public Uri Origination { get; set; }

        /// <summary>
        /// Specifies the base URI of the destination instance to which results should be 
        /// constrained
        /// </summary>
        public Uri Destination { get; set; }

        /// <summary>
        /// The full or partial message name to which results should be constrained
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// If specifiemd, filters results to include only messages related to (i.e. replies to)
        /// the specified message ID
        /// </summary>
        /// <seealso cref="IMessageHeaders.RelatedTo"/>
        /// <seealso cref="IMessageContext.SendReply"/>
        public MessageId? RelatedTo { get; set; }
    }
}
