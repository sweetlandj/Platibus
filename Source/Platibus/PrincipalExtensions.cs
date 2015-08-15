// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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

using System.Security.Principal;

namespace Platibus
{
    /// <summary>
    /// Helper methods for working with security <see cref="IPrincipal"/>s.
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// The name returned when the actual principal name cannot be
        /// determined
        /// </summary>
        public const string AnonymousPrincipalName = "(Unknown)";

        /// <summary>
        /// Returns the name of the principal or the 
        /// <see cref="AnonymousPrincipalName"/> if the principal is <c>null</c>
        /// or has a <c>null</c> identity
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <returns>Returns <see href="AnonymousPrincipalName"/> if the
        /// <paramref name="principal"/> is <c>null</c>; has a <c>null</c>
        /// identity; or has an identity whose name is <c>null</c> or 
        /// whitespace.  Otherwise returns the name of the identity
        /// associated with the <paramref name="principal"/>.</returns>
        public static string GetName(this IPrincipal principal)
        {
            if (principal == null || principal.Identity == null)
            {
                return AnonymousPrincipalName;
            }

            return string.IsNullOrWhiteSpace(principal.Identity.Name)
                ? AnonymousPrincipalName
                : principal.Identity.Name;
        }
    }
}