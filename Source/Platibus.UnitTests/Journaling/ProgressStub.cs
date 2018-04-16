using Platibus.Journaling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.UnitTests.Journaling
{
    public class ProgressStub : IProgress<MessageJournalConsumerProgress>
    {
        private readonly TaskCompletionSource<MessageJournalConsumerProgress> _progressSource;

        public Task<MessageJournalConsumerProgress> Report => _progressSource.Task;

        public ProgressStub(CancellationToken cancellationToken)
        {
            _progressSource = new TaskCompletionSource<MessageJournalConsumerProgress>();
            cancellationToken.Register(() => _progressSource.TrySetCanceled());
        }

        void IProgress<MessageJournalConsumerProgress>.Report(MessageJournalConsumerProgress value)
        {
            _progressSource.TrySetResult(value);
        }
    }
}