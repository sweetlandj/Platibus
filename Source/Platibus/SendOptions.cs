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
using Platibus.Serialization;

namespace Platibus
{
    /// <summary>
    /// Parameters that influence how a message is sent
    /// </summary>
    public struct SendOptions
    {
        /// <summary>
        /// The MIME type of the message content
        /// </summary>
        /// <remarks>
        /// This influences how the message will be serialized.  For example,
        /// if the content type is "application/json", then the body will be a
        /// serialized JSON string; if the content type is "application/xml"
        /// then the content will be an XML string; and if the content type is
        /// "application/octet-stream" then the content will be a base-64
        /// encoded byte stream.  Which content types are supported and how
        /// they are serialized depends on the <see cref="ISerializationService"/>
        /// used.
        /// </remarks>
        public string ContentType { get; set; }

        /// <summary>
        /// The maximum Time To Live (TTL) for the message.  When the TTL expires
        /// all attempts to deliver or handle the message will stop
        /// </summary>
        public TimeSpan TTL { get; set; }

        /// <summary>
        /// The importance of the message
        /// </summary>
        /// <remarks>
        /// Message importance influences whether messages are queued by the
        /// sender, queued by the receiver, or both.  For instance, 
        /// <see cref="MessageImportance.Critical"/> messages will be queued
        /// on both sides and every attempt will be made to deliver and handle
        /// them at least once.
        /// </remarks>
        public MessageImportance Importance { get; set; }
    }
}