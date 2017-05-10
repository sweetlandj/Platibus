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
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// Extension methods to make working with <see cref="ISecurityTokenService"/>
    /// implementations safer and more convenient
    /// </summary>
    public static class SecurityTokenServiceExtensions
    {
        /// <summary>
        /// Validates the specified <paramref name="token"/>, returning <c>null</c> if the token is
        /// null or whitespace
        /// </summary>
        /// <param name="service">The message security token service used to validate the token</param>
        /// <param name="token">The token to validate</param>
        /// <returns>Returns the validated <see cref="IPrincipal"/> if <see cref="token"/> is not
        /// null or whitespace; returns <c>null</c> otherwise</returns>
        public static async Task<IPrincipal> NullSafeValidate(this ISecurityTokenService service, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            return await service.Validate(token);
        }

        /// <summary>
        /// Issues a security token for the specified <paramref name="principal"/>, returning 
        /// <c>null</c> if the principal is null
        /// </summary>
        /// <param name="service">The message security token service used to validate the token</param>
        /// <param name="principal">The principal for which a security token is to be issued.  Can
        /// be <c>null</c>.</param>
        /// <param name="expires">(Optional) The date/time at which the issued token should expire</param>
        /// <returns>Returns a security token for the specified <see cref="principal"/> if it is
        /// not null; returns <c>null</c> otherwise</returns>
        public static async Task<string> NullSafeIssue(this ISecurityTokenService service, IPrincipal principal, DateTime? expires = null)
        {
            if (principal == null) return null;
            return await service.Issue(principal, expires);
        }
    }
}
