using System;

namespace Platibus
{
    /// <summary>
    /// Extension methods for working with <see cref="IObservable{T}"/> objects
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Subscribes to notifications produced by an observable object
        /// </summary>
        /// <typeparam name="TSource">The type of notification</typeparam>
        /// <param name="source">The observable source</param>
        /// <param name="handleNext">(Optional) An action delegate that will be
        /// invoked when the next notification is produced by the observable object</param>
        /// <param name="handleComplete">(Optional) An action delegate that will be
        /// invoked when all notifications have been produced</param>
        /// <param name="handleError">(Optional) An action delegate that will be
        /// invoked when an error notification is produced by the observable object</param>
        /// <returns>Returns a disposable object representing the subscription which,
        /// when disposed, will terminate the subscription</returns>
        public static IDisposable Subscribe<TSource>(this IObservable<TSource> source, Action<TSource> handleNext,
            Action handleComplete = null, Action<Exception> handleError = null)
        {
            var observer = new DelegateObserver<TSource>(handleNext, handleComplete, handleError);
            return source.Subscribe(observer);
        }

        private class DelegateObserver<TSource> : IObserver<TSource>
        {
            private readonly Action<TSource> _handleNext;
            private readonly Action _handleComplete;
            private readonly Action<Exception> _handleError;

            public DelegateObserver(Action<TSource> handleNext, Action handleComplete = null,
                Action<Exception> handleError = null)
            {
                _handleNext = handleNext;
                _handleComplete = handleComplete;
                _handleError = handleError;
            }

            public void OnCompleted()
            {
                if (_handleComplete != null) _handleComplete();
            }

            public void OnError(Exception error)
            {
                if (_handleError != null) _handleError(error);
            }

            public void OnNext(TSource value)
            {
                if (_handleNext != null) _handleNext(value);
            }
        }
    }
}