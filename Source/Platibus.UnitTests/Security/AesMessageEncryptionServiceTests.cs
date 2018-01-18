using System.Security.Cryptography;
using Platibus.Security;
#if NET452
using System.IdentityModel.Tokens;
#endif
#if NETCOREAPP2_0
using Microsoft.IdentityModel.Tokens;
#endif

namespace Platibus.UnitTests.Security
{
    public class AesMessageEncryptionServiceTests : MessageEncryptionServiceTests
    {
        public AesMessageEncryptionServiceTests()
            : base(InitMessageEncryptionService())
        {
        }

        private static IMessageEncryptionService InitMessageEncryptionService()
        {
            using (var csp = new AesCryptoServiceProvider())
            {
                csp.GenerateKey();
#if NET452
                var key = new InMemorySymmetricSecurityKey(csp.Key);
#endif
#if NETCOREAPP2_0
                var key = new SymmetricSecurityKey(csp.Key);
#endif
                return new AesMessageEncryptionService(key);
            }
        }
    }
}
