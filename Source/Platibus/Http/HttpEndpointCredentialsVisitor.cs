// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Platibus.Security;

namespace Platibus.Http
{
    internal class HttpEndpointCredentialsVisitor : IEndpointCredentialsVisitor
    {
        private readonly HttpClientHandler _clientHandler;
        private readonly HttpClient _client;

        public HttpEndpointCredentialsVisitor(HttpClientHandler clientHandler, HttpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");
            _clientHandler = clientHandler;
            _client = client;
        }

        public void Visit(BasicAuthCredentials credentials)
        {
            var authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        credentials.Username + ":" + credentials.Password)));

            _client.DefaultRequestHeaders.Authorization = authorization;
        }

        public void Visit(DefaultCredentials credentials)
        {
            _clientHandler.UseDefaultCredentials = true;
        }

        public void Visit(BearerCredentials credentials)
        {
            var authorization = new AuthenticationHeaderValue("Bearer", credentials.Credentials);
            _client.DefaultRequestHeaders.Authorization = authorization;
        }
    }
}