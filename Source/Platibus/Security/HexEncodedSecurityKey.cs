using System.IdentityModel.Tokens;

namespace Platibus.Security
{
    /// <summary>
    /// A <see cref="SecurityKey"/> implementation based on a bytes represented as hexadecimal
    /// degits
    /// </summary>
    public class HexEncodedSecurityKey : InMemorySymmetricSecurityKey
    {
        /// <summary>
        /// Initializes a new <see cref="HexEncodedSecurityKey"/> based on the specified
        /// <paramref name="hex"/> string representation
        /// </summary>
        /// <param name="hex">The hexadecimal string representation of the key bytes</param>
        public HexEncodedSecurityKey(string hex) : base(HexEncoding.GetBytes(hex))
        {
        }
    }
}
