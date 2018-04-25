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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// Thrown to indicate that there are no registered serializers suitable for
    /// a particular MIME content type
    /// </summary>
    [Serializable]
    public class SerializerNotFoundException : ApplicationException
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Serialization.SerializerNotFoundException" /> for the
        /// specified <paramref name="contentType" />
        /// </summary>
        /// <param name="contentType">The MIME content type for which a serializer
        /// was not found</param>
        public SerializerNotFoundException(string contentType) : base(contentType)
        {
            ContentType = contentType;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a serialized <see cref="T:Platibus.Serialization.SerializerNotFoundException" />
        /// from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public SerializerNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ContentType = info.GetString("ContentType");
        }

        /// <summary>
        /// The MIME content type for which a serializer was not found
        /// </summary>
        public string ContentType { get; }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ContentType", ContentType);
        }
    }
}