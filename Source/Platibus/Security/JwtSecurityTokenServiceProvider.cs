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

using System.Threading.Tasks;
using Platibus.Config.Extensibility;
#if NET452
using Platibus.Config;
#endif
#if NETSTANDARD2_0
using System;
using Microsoft.Extensions.Configuration;
#endif

namespace Platibus.Security
{
    /// <inheritdoc />
    /// <summary>
    /// Provides implementation of <see cref="T:Platibus.Security.ISecurityTokenService" /> based on 
    /// JSON Web Tokens (JWT)
    /// </summary>
    [Provider("JWT")]
    public class JwtSecurityTokenServiceProvider : ISecurityTokenServiceProvider
    {
#if NET452
        /// <inheritdoc />
        public Task<ISecurityTokenService> CreateSecurityTokenService(SecurityTokensElement configuration)
        {
            var signingKey = (HexEncodedSecurityKey)configuration.GetString("signingKey");
            var fallbackSigningKey = (HexEncodedSecurityKey)configuration.GetString("fallbackSigningKey");
            var defaultTtl = configuration.GetTimeSpan("defaultTtl");
            var options = new JwtSecurityTokenServiceOptions
            {
                SigningKey = signingKey,
                FallbackSigningKey = fallbackSigningKey,
                DefaultTTL = defaultTtl
            };
            var securityTokenService = new JwtSecurityTokenService(options);
            return Task.FromResult<ISecurityTokenService>(securityTokenService);
        }
#endif
#if NETSTANDARD2_0
        /// <inheritdoc />
        public Task<ISecurityTokenService> CreateSecurityTokenService(IConfiguration configuration)
        {
            var signingKey = (HexEncodedSecurityKey)configuration?["signingKey"];
            var fallbackSigningKey = (HexEncodedSecurityKey)configuration?["fallbackSigningKey"];
            
            var options = new JwtSecurityTokenServiceOptions
            {
                SigningKey = signingKey,
                FallbackSigningKey = fallbackSigningKey
            };

            var defaultTtl = configuration?.GetValue<TimeSpan>("defaultTtl");
            if (defaultTtl > TimeSpan.Zero)
            {
                options.DefaultTTL = defaultTtl.Value;
            }

            var securityTokenService = new JwtSecurityTokenService(options);
            return Task.FromResult<ISecurityTokenService>(securityTokenService);
        }
#endif
    }
}
