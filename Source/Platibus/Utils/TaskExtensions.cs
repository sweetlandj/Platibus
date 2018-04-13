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
        public static TResult GetResultFromCompletionSource<TResult>(this Task<TResult> task, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (task.IsCompleted) return task.Result;
            return task.GetCompletionSource(cancellationToken).Task.Result;
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
        public static void WaitOnCompletionSource(this Task task, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (task.IsCompleted) return;
            var completionSource = task.GetCompletionSource(cancellationToken);
            completionSource.Task.Wait(cancellationToken);
        }
        
        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{TResult}"/> that will be canceled, completed,
        /// or faulted according to the status of the specified antecedent <paramref name="task"/> via
        /// a continuation to avoid deadlock conditions in the 
        /// <see cref="System.Threading.SynchronizationContext"/> or
        /// <see cref="System.Threading.ExecutionContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected by the task</typeparam>
        /// <param name="task">The task to await</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to cancel the wait.</param>
        /// <returns>Returns a completion source that will be completed, canceled, or faulted according
        /// to the result of the specified antecedent <paramref name="task"/></returns>
        public static TaskCompletionSource<TResult> GetCompletionSource<TResult>(this Task<TResult> task, CancellationToken cancellationToken = default(CancellationToken))
        {
            var completionSource = new TaskCompletionSource<TResult>();
            if (!task.IsCompleted)
            {
                cancellationToken.Register(() => completionSource.TrySetCanceled());
            }
            
            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    completionSource.TrySetCanceled();
                }
                else if (t.Exception != null)
                {
                    completionSource.TrySetException(t.Exception.InnerExceptions);
                }
                else
                {
                    completionSource.TrySetResult(t.Result);
                }
            }, cancellationToken);
            return completionSource;
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{TResult}"/> that will be canceled, completed,
        /// or faulted according to the status of the specified antecedent <paramref name="task"/> via
        /// a continuation to avoid deadlock conditions in the 
        /// <see cref="System.Threading.SynchronizationContext"/> or
        /// <see cref="System.Threading.ExecutionContext"/>.
        /// </summary>
        /// <param name="task">The task to await</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to cancel the wait.</param>
        /// <returns>Returns a completion source that will be completed, canceled, or faulted according
        /// to the result of the specified antecedent <paramref name="task"/></returns>
        public static TaskCompletionSource<bool> GetCompletionSource(this Task task, CancellationToken cancellationToken = default(CancellationToken))
        {
            return task.GetCompletionSource(true, cancellationToken);
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{TResult}"/> that will be canceled, completed,
        /// or faulted according to the status of the specified antecedent <paramref name="task"/> via
        /// a continuation to avoid deadlock conditions in the 
        /// <see cref="System.Threading.SynchronizationContext"/> or
        /// <see cref="System.Threading.ExecutionContext"/>.
        /// </summary>
        /// <param name="task">The task to await</param>
        /// <param name="result">The result to return if the antecedent <paramref name="task"/> runs to completion</param>
        /// <param name="cancellationToken">(Optional) A cancellation token supplied by the
        /// caller that can be used to cancel the wait.</param>
        /// <returns>Returns a completion source that will be completed, canceled, or faulted according
        /// to the result of the specified antecedent <paramref name="task"/></returns>
        public static TaskCompletionSource<TResult> GetCompletionSource<TResult>(this Task task, TResult result, CancellationToken cancellationToken = default(CancellationToken))
        {
            var completionSource = new TaskCompletionSource<TResult>();
            if (!task.IsCompleted)
            {
                cancellationToken.Register(() => completionSource.TrySetCanceled());
            }
            
            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    completionSource.TrySetCanceled();
                }
                else if (t.Exception != null)
                {
                    completionSource.TrySetException(t.Exception.InnerExceptions);
                }
                else
                {
                    completionSource.TrySetResult(result);
                }
            }, cancellationToken);
            return completionSource;
        }
    }
}
