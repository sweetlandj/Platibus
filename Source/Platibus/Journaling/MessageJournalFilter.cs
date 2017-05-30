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
            get { return _categories ?? (_categories = new List<MessageJournalCategory>()); }
            set { _categories = value; }
        }

        /// <summary>
        /// Specifies the topics to which results should be constrained
        /// </summary>
        public IList<TopicName> Topics
        {
            get { return _topics ?? (_topics = new List<TopicName>()); }
            set { _topics = value; }
        }
    }
}
