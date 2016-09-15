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


using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Security
{
    /// <summary>
    /// A service that identifies whether requestors are authorized to perform
    /// certain operations or access certain resources
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Indicates whether the requestor is authorized to send messages
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to send messages;
        /// <c>false</c> otherwise</returns>
        Task<bool> IsAuthorizedToSendMessages(IPrincipal principal);

        /// <summary>
        /// Indicates whether the requestor is authorized to subscribe to the specified topic
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="topic">The topic</param>
        /// <returns>Returns <c>true</c> if the principal is authorized to subscribe to the
        /// topic; <c>false</c> otherwise</returns>
        Task<bool> IsAuthorizedToSubscribe(IPrincipal principal, TopicName topic);
    }
}
