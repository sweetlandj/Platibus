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
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace Platibus.Http
{
    /// <summary>
    /// An HTTP resource request
    /// </summary>
    public interface IHttpResourceRequest
    {
        /// <summary>
        /// The request URL
        /// </summary>
        Uri Url { get; }

        /// <summary>
        /// The HTTP method (GET, POST, PUT, DELETE, etc.)
        /// </summary>
        string HttpMethod { get; }

        /// <summary>
        /// HTTP headers
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// The parameters parsed from the URL query string
        /// </summary>
        NameValueCollection QueryString { get; }

        /// <summary>
        /// The MIME content type of the request content
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// The content encoding
        /// </summary>
        Encoding ContentEncoding { get; }

        /// <summary>
        /// A stream from which the request content can be read
        /// </summary>
        Stream InputStream { get; }

        /// <summary>
        /// The principal that made the request
        /// </summary>
        IPrincipal Principal { get; }
    }
}