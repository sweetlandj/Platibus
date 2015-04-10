using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    class SQLiteOperation<TResult>
    {
        private readonly Func<IDbConnection, TResult> _call;
        private readonly TaskCompletionSource<TResult> _taskCompletionSource;

        public Task<TResult> Task
        {
            get { return _taskCompletionSource.Task; }
        }

        public SQLiteOperation(Func<IDbConnection, TResult> call)
        {
            if (call == null) throw new ArgumentNullException("call");
            _call = call;
            _taskCompletionSource = new TaskCompletionSource<TResult>();
        }

        public void Execute(IDbConnection connection)
        {
            try
            {
                var result = _call(connection);
                _taskCompletionSource.SetResult(result);
            }
            catch(Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }
        }

        public void Cancel()
        {
            _taskCompletionSource.SetCanceled();
        }
    }
}
