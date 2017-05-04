using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;

namespace Platibus.Security
{
    /// <summary>
    /// A serializable security token that can be stored with a message that is persisted on a
    /// message queue.
    /// </summary>
    public class MessageSecurityToken
    {
        private readonly JwtSecurityToken _jwt;

        /// <summary>
        /// Initializes a new <see cref="MessageSecurityToken"/> for the specified 
        /// <paramref name="subject"/>
        /// </summary>
        /// <param name="subject">The principal whose claims are being represented by the
        /// serialized security token</param>
        private MessageSecurityToken(ClaimsIdentity subject)
        {
            var handler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = subject
            };
            _jwt = handler.CreateJwtSecurityToken(descriptor);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return new JwtSecurityTokenHandler().WriteToken(_jwt);
        }

        /// <summary>
        /// Creates a new <see cref="MessageSecurityToken"/> for the specified principal
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>Returns a message security token for the specified principal</returns>
        public static MessageSecurityToken Create(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            var identity = principal.Identity;
            var claimsIdentity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            return new MessageSecurityToken(claimsIdentity);
        }

        /// <summary>
        /// Validates the specified serialized <paramref name="messageToken"/> and produces a
        /// corresponding principal object
        /// </summary>
        /// <param name="messageToken">The message security token</param>
        /// <returns></returns>
        public static IPrincipal Validate(string messageToken)
        {
            if (messageToken == null) throw new ArgumentNullException("messageToken");
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                RequireSignedTokens = false,
                ValidateIssuer = false,
                ValidateAudience = false
            };
            SecurityToken token;
            return handler.ValidateToken(messageToken, parameters, out token);
        }

        /// <summary>
        /// Implicitly converts a <see cref="MessageSecurityToken"/> into its serialized
        /// string representation
        /// </summary>
        /// <param name="token">The message security token</param>
        public static implicit operator string(MessageSecurityToken token)
        {
            return token == null ? null : token.ToString();
        }
    }
}
