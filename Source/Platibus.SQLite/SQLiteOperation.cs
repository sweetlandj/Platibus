using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    class SQLiteOperation : ISQLiteOperation
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private readonly Task _task;
        private readonly Func<Task> _asyncOperation;

        public Task Task
        {
            get { return _task; }
        }

        public SQLiteOperation(Action operation)
            : this(AsAsync(operation))
        {
        }

        private static Func<Task> AsAsync(Action operation)
        {
            if (operation == null) throw new ArgumentNullException("operation");
            return () =>
            {
                operation();
                return System.Threading.Tasks.Task.FromResult(true);
            };
        }

        public SQLiteOperation(Func<Task> asyncOperation)
        {
            if (asyncOperation == null) throw new ArgumentNullException("asyncOperation");
            _taskCompletionSource = new TaskCompletionSource<bool>(false);
            _task = _taskCompletionSource.Task;
            _asyncOperation = asyncOperation;
        }

        public async Task Execute()
        {
            try
            {
                await _asyncOperation();
                _taskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }
        }
    }

    class SQLiteOperation<TResult> : ISQLiteOperation
    {
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;
        private readonly Task<TResult> _task;
        private readonly Func<Task<TResult>> _asyncOperation;
        
        public Task<TResult> Task
        {
            get { return _task; }
        }

        public SQLiteOperation(Func<TResult> operation)
            : this(AsAsync(operation))
        {
        }

        private static Func<Task<TResult>> AsAsync(Func<TResult> operation)
        {
            if (operation == null) throw new ArgumentNullException("operation");
            return () => System.Threading.Tasks.Task.FromResult(operation());
        }

        public SQLiteOperation(Func<Task<TResult>> asyncOperation)
        {
            if (asyncOperation == null) throw new ArgumentNullException("asyncOperation");
            _taskCompletionSource = new TaskCompletionSource<TResult>(false);
            _task = _taskCompletionSource.Task;
            _asyncOperation = asyncOperation;
        }

        public async Task Execute()
        {
            try
            {
                var result = await _asyncOperation();
                _taskCompletionSource.SetResult(result);
            }
            catch(Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }
        }
    }
}
