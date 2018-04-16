using Platibus.Journaling;
using System;

namespace Platibus.UnitTests.Journaling
{
    public class ProgressStub : IProgress<MessageJournalConsumerProgress>
    {
        private readonly object _syncRoot = new object();
        public MessageJournalConsumerProgress Report { get; private set; }

        void IProgress<MessageJournalConsumerProgress>.Report(MessageJournalConsumerProgress value)
        {
            lock(_syncRoot)
            {
                Report = value;
            }
        }
    }
}