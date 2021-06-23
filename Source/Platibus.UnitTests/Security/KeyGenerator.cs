using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Platibus.UnitTests.Security
{
    internal static class KeyGenerator
    {
        public static SymmetricSecurityKey GenerateAesKey()
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                csp.KeySize = 256;
                csp.GenerateKey();
                return new SymmetricSecurityKey(csp.Key);
            }
        }
    }
}
