// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus
{
    [Serializable]
    public class EndpointNotFoundException : ApplicationException
    {
        private readonly EndpointName _endpoint;
        private readonly Uri _uri;

        public EndpointNotFoundException(EndpointName endpoint) : base(endpoint)
        {
            _endpoint = endpoint;
        }

        public EndpointNotFoundException(Uri uri) : base(uri == null ? null : uri.ToString())
        {
            _uri = uri;
        }

        public EndpointNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _endpoint = info.GetString("endpoint");
            var uriStr = info.GetString("uri");
            _uri = string.IsNullOrWhiteSpace(uriStr) ? null : new Uri(uriStr);
        }

        public EndpointName Endpoint
        {
            get { return _endpoint; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("endpoint", (string) _endpoint);
            info.AddValue("uri", _uri == null ? null : _uri.ToString());
        }
    }
}