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

using System.IO;
using System.Text;

namespace Platibus.Http
{
    /// <summary>
    /// A response to a request pertaining to an HTTP resource
    /// </summary>
    /// <seealso cref="IHttpResourceRequest"/>
    public interface IHttpResourceResponse
    {
        /// <summary>
        /// The HTTP response status code
        /// </summary>
        int StatusCode { set; }

        /// <summary>
        /// The HTTP response status description
        /// </summary>
        string StatusDescription { set; }

        /// <summary>
        /// The output stream to which response content is written
        /// </summary>
        Stream OutputStream { get; }

        /// <summary>
        /// The MIME content type of the response content
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// The character encoding of the response content
        /// </summary>
        Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Adds a header value to the response
        /// </summary>
        /// <param name="header">The name of the header</param>
        /// <param name="value">The header value</param>
        void AddHeader(string header, string value);
    }
}