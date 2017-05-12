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
using System.Security.Principal;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IntegrationTests
{
    internal class TestAuthorizationService : BasicAuthorizationService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly bool _canSend;
        private readonly bool _canSubscribe;

        public TestAuthorizationService(string username, string password, bool canSend, bool canSubscribe)
        {
            _username = username;
            _password = password;
            _canSend = canSend;
            _canSubscribe = canSubscribe;
        }

        protected override Task<bool> Authenticate(string username, string password)
        {
            var authenticated = string.Equals(username, _username, StringComparison.OrdinalIgnoreCase)
                   && Equals(password, _password);

            return Task.FromResult(authenticated);
        }

        public override async Task<bool> IsAuthorizedToSendMessages(IPrincipal principal)
        {
            return await base.IsAuthorizedToSendMessages(principal) && _canSend;
        }

        public override async Task<bool> IsAuthorizedToSubscribe(IPrincipal principal, TopicName topic)
        {
            return await base.IsAuthorizedToSubscribe(principal, topic) && _canSubscribe;
        }
    }
}
