using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    public class SynchronizingCommandExecutor : ICommandExecutor, IDisposable
    {
        private readonly SemaphoreSlim _synchronization = new SemaphoreSlim(1);

        private bool _disposed;
        
        public async Task Execute(Func<Task> command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            CheckDisposed();
            await _synchronization.WaitAsync(cancellationToken);
            try
            {
                await command();
            }
            finally
            {
                _synchronization.Release();
            }
        }

        public async Task<TResult> ExecuteRead<TResult>(Func<Task<TResult>> command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            CheckDisposed();
            await _synchronization.WaitAsync(cancellationToken);
            try
            {
                return await command();
            }
            finally
            {
                _synchronization.Release();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _synchronization.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        ~SynchronizingCommandExecutor()
        {
            Dispose(false);
        }
    }
}
