namespace Platibus.Security
{
    /// <summary>
    /// Extension methods for working with message security tokens
    /// </summary>
    public static class MessageSecurityTokenExtensions
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
        /// <seealso cref="IMessageSecurityTokenService.Issue"/>
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
