using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Utils
{
    /// <summary>
    /// Extensions for working with tasks
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Uses continuations and <see cref="TaskCompletionSource{TResult}"/> to await
        /// the results of a task without causing deadlock in the 
        /// <see cref="System.Threading.SynchronizationContext"/> or
        /// <see cref="System.Threading.ExecutionContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected by the task</typeparam>
        /// <param name="task">The task to await</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to cancel the wait.</param>
        /// <returns>Returns the result of the task</returns>
        public static TResult GetResultUsingContinuation<TResult>(this Task<TResult> task, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (task.IsCompleted) return task.Result;
            var tcs = new TaskCompletionSource<TResult>();
            cancellationToken.Register(() => tcs.TrySetCanceled());
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception is AggregateException aex)
                    {
                        tcs.TrySetException(aex.InnerExceptions);
                    }
                    else
                    {
                        tcs.TrySetException(t.Exception);
                    }
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            }, cancellationToken);
            return tcs.Task.Result;
        }

        /// <summary>
        /// Uses continuations and <see cref="TaskCompletionSource{TResult}"/> to await
        /// the results of a task without causing deadlock in the 
        /// <see cref="System.Threading.SynchronizationContext"/> or
        /// <see cref="System.Threading.ExecutionContext"/>.
        /// </summary>
        /// <param name="task">The task to await</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to cancel the wait.</param>
        public static void WaitUsingContinuation(this Task task, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (task.IsCompleted) return;
            var tcs = new TaskCompletionSource<int>();
            cancellationToken.Register(() => tcs.TrySetCanceled());
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    tcs.TrySetException(t.Exception);
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(1);
                }
            }, cancellationToken);
            tcs.Task.Wait(cancellationToken);
        }
    }
}
