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
		/// Ensures that the path of the specified <paramref name="uri"/> contains a trailing slash
		/// </summary>
		/// <param name="uri">The URI</param>
		/// <returns>Returns the specified URI with a trailing slash</returns>
	    public static Uri WithTrailingSlash(this Uri uri)
		{
		    if (uri == null) return null;
		    if (uri.AbsolutePath.EndsWith("/")) return uri;
		    return new UriBuilder(uri)
		    {
			    Path = uri.AbsolutePath + "/"
		    }.Uri;
	    }

        /// <summary>
		/// Ensures that the path of the specified <paramref name="uri"/> contains a trailing slash
		/// </summary>
		/// <param name="uri">The URI</param>
		/// <returns>Returns the specified URI with a trailing slash</returns>
	    public static Uri WithoutTrailingSlash(this Uri uri)
        {
            if (uri == null) return null;
            if (!uri.AbsolutePath.EndsWith("/")) return uri;
            return new UriBuilder(uri)
            {
                Path = uri.AbsolutePath.TrimEnd('/')
            }.Uri;
        }

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

        /// <summary>
        /// Waits for a task to complete until a timeout is reached,
        /// catching any exceptions that occur
        /// </summary>
        /// <param name="t">The task to wait on</param>
        /// <param name="timeout">The maximum amount of time to wait</param>
        /// <returns>Returns <c>true</c> if the task completed or was
        /// canceled; <c>false</c> otherwise</returns>
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