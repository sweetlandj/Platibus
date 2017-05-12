// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
    }
}