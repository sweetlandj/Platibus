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
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus.Config
{
    /// <summary>
    /// Exception thrown to indicate that the same topic name has been added
    /// more than once
    /// </summary>
    [Serializable]
    public class TopicAlreadyExistsException : ApplicationException
    {
        private readonly TopicName _topic;

        /// <summary>
        /// Initializes a new <see cref="TopicAlreadyExistsException"/> with
        /// the specified <paramref name="topic"/>
        /// </summary>
        /// <param name="topic">The name of the topic that was added multiple
        /// time</param>
        public TopicAlreadyExistsException(TopicName topic)
        {
            _topic = topic;
        }

        /// <summary>
        /// Initializes a serialized <see cref="TopicAlreadyExistsException"/>
        /// from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public TopicAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _topic = info.GetString("Topic");
        }

        /// <summary>
        /// The name of the topic that was added multiple times
        /// </summary>
        public TopicName Topic
        {
            get { return _topic; }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Topic", _topic);
        }
    }
}