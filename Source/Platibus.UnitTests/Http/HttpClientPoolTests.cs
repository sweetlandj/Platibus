using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Platibus.Http;
using Platibus.Security;

namespace Platibus.UnitTests.Http
{
    public class HttpClientPoolTests
    {
       [Fact]
        public async Task HttpClientPoolAllocatesOneHandlerPerUriAndManyCredentialsOfTheSameType()
        {
            var uri = new Uri("http://localhost/platibus");
            var clientPool = new HttpClientPool();

            var credentialSet = Enumerable.Range(0, 100)
                .Select(i => new BasicAuthCredentials("user" + i, "pw" + i));

            var initialSize = clientPool.Size;
            foreach (var credentials in credentialSet)
            {
                await clientPool.GetClient(uri, credentials);
            }

            var finalSize = clientPool.Size;
            clientPool.Dispose();

            Assert.Equal(1, finalSize - initialSize);
        }

       [Fact]
        public async Task HttpClientPoolAllocatesOneHandlerPerUriAndCredentialType()
        {
            var uri = new Uri("http://localhost/platibus");
            var clientPool = new HttpClientPool();

            var credentialSet = new IEndpointCredentials[]
            {
                new BasicAuthCredentials("user1", "pw1"),
                new BearerCredentials("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ"),
                new DefaultCredentials()
            };

            var initialSize = clientPool.Size;
            foreach (var credentials in credentialSet)
            {
                await clientPool.GetClient(uri, credentials);
            }

            var finalSize = clientPool.Size;
            clientPool.Dispose();
             
            Assert.Equal(3, finalSize - initialSize);
        }
    }
}
