using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.Security;

namespace Platibus.Journaling
{
    /// <summary>
    /// A wrapper for <see cref="IMessageJournal"/> implementations that ensures sensitive
    /// information is not written to the journal
    /// </summary>
    internal class SanitizedMessageJournal : IMessageJournal, IDisposable
    {
        private readonly IMessageJournal _inner;
        private bool _disposed;

        /// <summary>
        /// Initializes a new <see cref="SanitizedMessageJournal"/> wrapping the specified
        /// <paramref name="journal"/>
        /// </summary>
        /// <param name="journal">The message journal to wrap</param>
        public SanitizedMessageJournal(IMessageJournal journal)
        {
            if (journal == null) throw new ArgumentNullException("journal");
            _inner = journal;
        }

        /// <inheritdoc />
        public Task<MessageJournalPosition> GetBeginningOfJournal(CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.GetBeginningOfJournal(cancellationToken);
        }

        /// <inheritdoc />
        public Task Append(Message message, MessageJournalCategory category,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.Append(message.WithSanitizedHeaders(), category, cancellationToken);
        }

        /// <inheritdoc />
        public Task<MessageJournalReadResult> Read(MessageJournalPosition start, int count, MessageJournalFilter filter = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.Read(start, count, filter, cancellationToken);
        }

        /// <inheritdoc />
        public MessageJournalPosition ParsePosition(string str)
        {
            return _inner.ParsePosition(str);
        }

        /// <inheritdoc />
        ~SanitizedMessageJournal()
        {
            if (_disposed) return;
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var disposableJournal = _inner as IDisposable;
                if (disposableJournal != null)
                {
                    disposableJournal.Dispose();
                }
            }
        }
    }
}
