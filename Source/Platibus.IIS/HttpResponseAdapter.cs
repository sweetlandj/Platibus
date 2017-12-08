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
using System.IO;
using System.Text;
using System.Web;
using Platibus.Http;

namespace Platibus.IIS
{
    internal class HttpResponseAdapter : IHttpResourceResponse
    {
        private readonly HttpResponseBase _response;

        public int StatusCode
        {
            set => _response.StatusCode = value;
        }

        public string StatusDescription
        {
            set => _response.StatusDescription = value;
        }

        public string ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public Encoding ContentEncoding
        {
            get => _response.ContentEncoding;
            set => _response.ContentEncoding = value;
        }

        public Stream OutputStream => _response.OutputStream;

        public HttpResponseAdapter(HttpResponseBase response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public void AddHeader(string header, string value)
        {
            _response.AddHeader(header, value);
        }
    }
}