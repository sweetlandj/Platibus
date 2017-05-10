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

namespace Platibus.Security
{
    /// <summary>
    /// Extension methods for working with security tokens
    /// </summary>
    public static class SecurityTokenExtensions
    {
        /// <summary>
        /// Ensures that the message has a <see cref="IMessageHeaders.SecurityToken"/> header with
        /// the specified <paramref name="securityToken"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="securityToken">The security token</param>
        /// <returns>Returns the specified <paramref name="message"/> if the
        /// <see cref="IMessageHeaders.SecurityToken"/> header is present and equivalent to the
        /// supplied <paramref name="securityToken"/>.  Otherwise, returns a new 
        /// <see cref="Message"/> with the same headers and content plus a 
        /// <see cref="IMessageHeaders.SecurityToken"/> with the specified 
        /// <paramref name="securityToken"/>.</returns>
        /// <seealso cref="ISecurityTokenService.Issue"/>
        public static Message WithSecurityToken(this Message message, string securityToken)
        {
            var normalizedToken = string.IsNullOrWhiteSpace(securityToken)
                ? null
                : securityToken.Trim();

            if (message.Headers.SecurityToken == normalizedToken) return message;

            var updatedHeaders = new MessageHeaders(message.Headers)
            {
                SecurityToken = normalizedToken
            };
            return new Message(updatedHeaders, message.Content);
        }
    }
}
