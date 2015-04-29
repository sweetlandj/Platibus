using System;

namespace Platibus
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<TSource>(this IObservable<TSource> source, Action<TSource> handleNext, Action handleComplete = null, Action<Exception> handleError = null)
        {
            var observer = new DelegateObserver<TSource>(handleNext, handleComplete, handleError);
            return source.Subscribe(observer);
        }

        private class DelegateObserver<TSource> : IObserver<TSource>
        {
            private readonly Action<TSource> _handleNext;
            private readonly Action _handleComplete;
            private readonly Action<Exception> _handleError;

            public DelegateObserver(Action<TSource> handleNext, Action handleComplete = null, Action<Exception> handleError = null)
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
