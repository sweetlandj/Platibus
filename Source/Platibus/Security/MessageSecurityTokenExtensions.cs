using System.Security.Principal;

namespace Platibus.Security
{
    /// <summary>
    /// Extension methods for working with <see cref="MessageSecurityToken"/>s
    /// </summary>
    public static class MessageSecurityTokenExtensions
    {
        /// <summary>
        /// Ensures that the message has a <see cref="IMessageHeaders.SecurityToken"/> header that
        /// represents the specified <paramref name="principal"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="principal">The principal</param>
        /// <returns>Returns the specified <paramref name="message"/> if the
        /// <see cref="IMessageHeaders.SecurityToken"/> header is present and equivalent to the
        /// supplied <paramref name="principal"/>.  Otherwise, returns a new <see cref="Message"/>
        /// with the same headers and content plus a <see cref="IMessageHeaders.SecurityToken"/>
        /// header that represents the specified <paramref name="principal"/>.</returns>
        public static Message WithSecurityToken(this Message message, IPrincipal principal)
        {
            var securityToken = principal == null ? null : MessageSecurityToken.Create(principal);
            if (message.Headers.SecurityToken == securityToken) return message;

            var updatedHeaders = new MessageHeaders(message.Headers)
            {
                SecurityToken = securityToken
            };

            return new Message(updatedHeaders, message.Content);
        }
    }
}
