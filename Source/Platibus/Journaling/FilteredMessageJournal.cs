using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Journaling
{
    internal class FilteredMessageJournal : IMessageJournal
    {
        private readonly IMessageJournal _inner;
        private readonly IList<JournaledMessageCategory> _categories;

        public FilteredMessageJournal(IMessageJournal inner, IEnumerable<JournaledMessageCategory> categories = null)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            _inner = inner;
            _categories = (categories ?? Enumerable.Empty<JournaledMessageCategory>()).ToList();
        }

        public Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.GetBeginningOfJournal(cancellationToken);
        }

        public Task Append(Message message, JournaledMessageCategory category,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _categories.Contains(category) 
                ? _inner.Append(message, category, cancellationToken) 
                : Task.FromResult(0);
        }

        public Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.Read(start, count, filter, cancellationToken);
        }

        public MessageJournalPosition ParsePosition(string str)
        {
            return _inner.ParsePosition(str);
        }
    }
}
