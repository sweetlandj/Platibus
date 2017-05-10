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
    /// An interface describing an object that can generate a security token for a 
    /// <see cref="System.Security.Principal.IPrincipal"/> or validate a previously generated
    /// security token.
    /// </summary>
    public interface ISecurityTokenService
    {
        /// <summary>
        /// Issues a new security token representing the specified
        /// <paramref name="principal"/>
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="expires">(Optional) The date/time at which the token should expire</param>
        /// <returns>Returns a task whose result is a message security token representing the
        /// specified <paramref name="principal"/></returns>
        Task<string> Issue(IPrincipal principal, DateTime? expires = null);

        /// <summary>
        /// Validates a previously issued security token
        /// </summary>
        /// <param name="securityToken">The message security token</param>
        /// <returns>Returns the principal represented b the specified 
        /// <paramref name="securityToken"/></returns>
        Task<IPrincipal> Validate(string securityToken);
    }
}
