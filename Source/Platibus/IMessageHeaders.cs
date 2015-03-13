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
using System.Collections.Generic;

namespace Platibus
{
    public interface IMessageHeaders : IEnumerable<KeyValuePair<HeaderName, string>>
    {
        string this[HeaderName header] { get; }
        MessageId MessageId { get; }
        MessageName MessageName { get; }
        DateTime Expires { get; }
        Uri Origination { get; }
        Uri ReplyTo { get; }
        Uri Destination { get; }
        MessageId RelatedTo { get; }
        DateTime Published { get; }
        TopicName Topic { get; }
        DateTime Sent { get; }
        DateTime Received { get; }
        string ContentType { get; }
        Uri GetUri(HeaderName headerName);
        DateTime? GetDateTime(HeaderName headerName);
    }
}