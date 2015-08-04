// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Platibus.Http
{
    public class MessageController : IHttpResourceController
    {
        private readonly Func<Message, IPrincipal, Task> _accept;

        public MessageController(Func<Message, IPrincipal, Task> accept)
        {
            if (accept == null) throw new ArgumentNullException("accept");
            _accept = accept;
        }

        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response,
            IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            if (!request.IsPost())
            {
                response.StatusCode = 405;
                response.AddHeader("Allow", "POST");
                return;
            }

            await Post(request, response);
        }

        public async Task Post(IHttpResourceRequest request, IHttpResourceResponse response)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");

            var flattenedHeaders = request.Headers.AllKeys
                .ToDictionary(k => k, k => request.Headers[k]);

            var messageHeaders = new MessageHeaders(flattenedHeaders);
            var content = await request.ReadContentAsString();
            var message = new Message(messageHeaders, content);

            await _accept(message, request.Principal);
            response.StatusCode = 202;
        }
    }
}