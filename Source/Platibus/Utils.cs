using System;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus
{
    /// <summary>
    /// Helper extension methods for working with objects
    /// </summary>
    public static class Utils
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Core);

        /// <summary>
        /// Attempts to safely cast <paramref name="obj"/> to <see cref="IDisposable"/>
        /// and dispose it, catching and quietly logging exceptions.
        /// </summary>
        /// <param name="obj">The object to dispose</param>
        /// <returns>Returns <c>true</c> if <paramref name="obj"/> implements
        /// <see cref="IDisposable"/> and was successfully disposed; <c>false</c>
        /// otherwise.</returns>
        public static bool TryDispose(this object obj)
        {
            var disposableObject = obj as IDisposable;
            if (disposableObject == null) return false;

            try
            {
                disposableObject.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Unhandled exception disposing object of type {0}", ex, obj.GetType().FullName);
                return false;
            }
        }

        public static bool TryWait(this Task t, TimeSpan timeout)
        {
            if (t == null) return false;
            if (t.IsCompleted || t.IsCanceled) return false;

            try
            {
                t.Wait(timeout);
                return true;
            }
            catch (TaskCanceledException)
            {
                return true;
            }
            catch (Exception ex)
            {
                Log.InfoFormat("Unhandled exception in task {0}", ex, t.Id);
                return false;
            }
        }
    }
}