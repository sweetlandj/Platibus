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

using System.Security.Claims;
using System.Security.Principal;

namespace Platibus.Security
{
    /// <summary>
    /// Helper methods for working with <see cref="System.Security.Principal.IPrincipal"/>
    /// implementations
    /// </summary>
    public static class PrincipalExtensions
    {
        /// <summary>
        /// Returns a claim from the principal
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="claimType">The type of claim</param>
        /// <returns>The value of the specified claim as a string if present; <c>null</c>
        /// otherwise</returns>
        public static string GetClaimValue(this IPrincipal principal, string claimType)
        {
            if (principal == null) return null;
            var claimsIdentity = principal.Identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;

            var claim = claimsIdentity.FindFirst(claimType);
            return claim == null ? null : claim.Value;
        }
    }
}
