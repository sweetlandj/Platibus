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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace Platibus.Http
{
    internal class HttpListenerRequestAdapter : IHttpResourceRequest
    {
        private readonly HttpListenerRequest _request;
        private readonly IPrincipal _principal;

        public Uri Url
        {
            get { return _request.Url; }
        }

        public string HttpMethod
        {
            get { return _request.HttpMethod; }
        }

        public NameValueCollection Headers
        {
            get { return _request.Headers; }
        }

        public NameValueCollection QueryString
        {
            get { return _request.QueryString; }
        }

        public string ContentType
        {
            get { return _request.ContentType; }
        }

        public Encoding ContentEncoding
        {
            get { return _request.ContentEncoding; }
        }

        public Stream InputStream
        {
            get { return _request.InputStream; }
        }

        public IPrincipal Principal
        {
            get { return _principal; }
        }

        public HttpListenerRequestAdapter(HttpListenerRequest request, IPrincipal principal)
        {
            if (request == null) throw new ArgumentNullException("request");
            _request = request;
            _principal = principal;
        }
    }
}