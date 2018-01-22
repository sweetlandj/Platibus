#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETCOREAPP2_0
using Microsoft.IdentityModel.Tokens;
#endif
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
#if NET452
                return new InMemorySymmetricSecurityKey(csp.Key);
#endif
#if NETCOREAPP2_0
                return new SymmetricSecurityKey(csp.Key);
#endif
            }
        }
    }
}
