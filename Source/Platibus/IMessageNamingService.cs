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

namespace Platibus
{
    /// <summary>
    /// A service that maps CLR types onto message names (and vice versa) to
    /// convey type information in remote instances to aid with deserialization
    /// of message content
    /// </summary>
    public interface IMessageNamingService
    {
        /// <summary>
        /// Returns the canonical message name for a given type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>The canonical name associated with the type</returns>
        MessageName GetNameForType(Type messageType);

        /// <summary>
        /// Returns the type associated with a canonical message name
        /// </summary>
        /// <param name="messageName">The canonical message name</param>
        /// <returns>The type best suited to deserialized content for messages
        /// with the specified <paramref name="messageName"/></returns>
        Type GetTypeForName(MessageName messageName);
    }
}