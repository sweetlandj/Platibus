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

using Microsoft.Owin;

namespace Platibus.Owin
{
    /// <summary>
    /// Extension methods for working with Platibus components within an HTTP context
    /// </summary>
    public static class OwinContextExtensions
    {
        /// <summary>
        /// Returns the Platibus bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>Returns the Platibus bus instance for the current HTTP context</returns>
        public static IBus GetBus(this IOwinContext context)
        {
            if (context == null) return null;
            return context.Get<IBus>(OwinContextItemKeys.Bus);
        }

        /// <summary>
        /// Used internally to intialize the bus instance for the current HTTP context
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="bus">The bus instance in scope for the HTTP context</param>
        internal static void SetBus(this IOwinContext context, Bus bus)
        {
            context.Set(OwinContextItemKeys.Bus, bus);
        }
    }
}
