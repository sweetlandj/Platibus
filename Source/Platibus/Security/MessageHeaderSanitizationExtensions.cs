using System;
using System.Linq;
using System.Net;

namespace Platibus.Security
{
    /// <summary>
    /// Methods for removing sensitive information from message headers
    /// </summary>
    public static class MessageHeaderSanitizationExtensions
    {
        /// <summary>
        /// Returns a copy of the specified <paramref name="message"/> with sanitized headers
        /// </summary>
        /// <param name="message">The message for which headers are to be sanitized</param>
        /// <returns>Returns a copy of the specified <paramref name="message"/> with sanitized 
        /// headers</returns>
        public static Message WithSanitizedHeaders(this Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            var sanitizedHeaders = message.Headers.Sanitize();
            return new Message(sanitizedHeaders, message.Content);
        }

        /// <summary>
        /// Returns a sanitized version of the specified <paramref name="headers"/>
        /// </summary>
        /// <param name="headers">The headers to sanitize</param>
        /// <returns>Returns the specified <paramref name="headers"/> with sensitive information
        /// removed</returns>
        public static IMessageHeaders Sanitize(this IMessageHeaders headers)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            
            var authorizationHeader = HttpRequestHeader.Authorization.ToString("G");
            var platibusSecurityTokenHeader = HeaderName.SecurityToken;
            var sensitiveHeaderNames = new HeaderName[]
            {
                authorizationHeader,
                platibusSecurityTokenHeader
            };

            var sanitizedHeaders = headers
                .Where(h => !sensitiveHeaderNames.Contains(h.Key));

            return new MessageHeaders(sanitizedHeaders);
        }
    }
}
