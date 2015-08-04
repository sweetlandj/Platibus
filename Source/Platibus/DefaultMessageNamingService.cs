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
using System.Collections.Concurrent;
using System.Linq;

namespace Platibus
{
    /// <summary>
    /// A basic implementation of <see cref="IMessageNamingService"/> that used
    /// the full name of the message type as the message name
    /// </summary>
    public class DefaultMessageNamingService : IMessageNamingService
    {
        private readonly ConcurrentDictionary<MessageName, Type> _messageTypesByName = new ConcurrentDictionary<MessageName, Type>();

        /// <summary>
        /// Returns the canonical message name for a given type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>The canonical name associated with the type</returns>
        public MessageName GetNameForType(Type messageType)
        {
            return messageType.FullName;
        }

        /// <summary>
        /// Returns the type associated with a canonical message name
        /// </summary>
        /// <param name="messageName">The canonical message name</param>
        /// <returns>The type best suited to deserialized content for messages
        /// with the specified <paramref name="messageName"/></returns>
        public Type GetTypeForName(MessageName messageName)
        {
            return _messageTypesByName.GetOrAdd(messageName, name => FindTypeByName(name));
        }

        private static Type FindTypeByName(string typeName)
        {
            return Type.GetType(typeName) ?? AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(t => t != null);
        }
    }
}