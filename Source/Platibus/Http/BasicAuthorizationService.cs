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


using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Security;

namespace Platibus.Http
{
    /// <summary>
    /// An abstract base class that enables implementors to verify basic HTTP auth
    /// credentials and determine whether the user is authorized to access the
    /// requested resource
    /// </summary>
    public abstract class BasicAuthorizationService : IAuthorizationService
    {
        /// <summary>
        /// Indicates whether the requestor is authorized to send messages
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to send messages;
        /// <c>false</c> otherwise</returns>
        /// <remarks>
        /// Extracts the basic HTTP credentials from the principal and returns the result
        /// of <see cref="Authenticate(string, string)"/>.  Implementors can override this
        /// method to provide additional authorization checks if required.
        /// </remarks>
        public virtual Task<bool> IsAuthorizedToSendMessages(IPrincipal principal)
        {
            return Authenticate(principal);
        }

        /// <summary>
        /// Indicates whether the requestor is authorized to subscribe to the specified topic
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="topic">The topic</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to subscribe to the
        /// topic; <c>false</c> otherwise</returns>
        /// <remarks>
        /// Extracts the basic HTTP credentials from the principal and returns the result
        /// of <see cref="Authenticate(string, string)"/>.  Implementors can override this
        /// method to provide additional authorization checks if required.
        /// </remarks>
        public virtual Task<bool> IsAuthorizedToSubscribe(IPrincipal principal, TopicName topic)
        {
            return Authenticate(principal);
        }

        private Task<bool> Authenticate(IPrincipal principal)
        {
            var identity = principal == null ? null : principal.Identity;
            var basicIdentity = identity as HttpListenerBasicIdentity;
            var username = identity == null ? null : identity.Name;
            var password = basicIdentity == null ? null : basicIdentity.Password;
            return Authenticate(username, password);
        }

        /// <summary>
        /// Authenticates the credentials supplied by the requestor
        /// </summary>
        /// <param name="username">The username specified in the request</param>
        /// <param name="password">The password specified in the request</param>
        /// <returns>Returns <c>true</c> if the requestor's credentials are valid;
        /// <c>false</c> otherwise</returns>
        protected abstract Task<bool> Authenticate(string username, string password);
    }
}
