// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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

namespace Platibus
{
    /// <summary>
    /// Thrown to indicate that a message was not sent because there were not
    /// matching send rules.
    /// </summary>
    [Serializable]
    public class NoMatchingSendRulesException : ApplicationException
    {
        /// <summary>
        /// Initializes a new <see cref="NoMatchingSendRulesException"/>
        /// </summary>
        public NoMatchingSendRulesException()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="NoMatchingSendRulesException"/>
        /// with the specified detail message
        /// </summary>
        /// <param name="message">The detail message</param>
        public NoMatchingSendRulesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="NoMatchingSendRulesException"/>
        /// with the specified detail message and nested exception
        /// </summary>
        /// <param name="message">The detail message</param>
        /// <param name="innerException">The nested exception</param>
        public NoMatchingSendRulesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a serialized <see cref="NoMatchingSendRulesException"/>
        /// from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public NoMatchingSendRulesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}