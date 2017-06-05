// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Platibus.Http;
using Platibus.Security;

namespace Platibus.UnitTests.Http
{
    [Trait("Category", "UnitTests")]
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
