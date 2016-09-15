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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;

namespace Platibus
{
    /// <summary>
    /// A message naming service based on known types annotated with data contracts
    /// </summary>
    /// <seealso cref="DataContractAttribute"/>
    public class DataContractMessageNamingService : IMessageNamingService
    {
        private readonly ConcurrentDictionary<Type, MessageName> _messageNamesByType = new ConcurrentDictionary<Type, MessageName>();
        private readonly ConcurrentDictionary<MessageName, Type> _typesByMessageName = new ConcurrentDictionary<MessageName, Type>();

        /// <summary>
        /// Adds a known message type 
        /// </summary>
        /// <param name="messageType">The known message type</param>
        public void Add(Type messageType)
        {
            var messageName = GetSchemaTypeName(messageType);
            _messageNamesByType.TryAdd(messageType, messageName);
            _typesByMessageName.TryAdd(messageName, messageType);
        }

        /// <summary>
        /// Returns the canonical message name for a given type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>The canonical name associated with the type</returns>
        /// <remarks>
        /// For types annotated with <see cref="DataContractAttribute"/>, the
        /// message name will be the schema type name
        /// </remarks>
        public MessageName GetNameForType(Type messageType)
        {
            return _messageNamesByType.GetOrAdd(messageType, GetSchemaTypeName);
        }

        /// <summary>
        /// Returns the type associated with a canonical message name
        /// </summary>
        /// <param name="messageName">The canonical message name</param>
        /// <returns>The type best suited to deserialized content for messages
        /// with the specified <paramref name="messageName"/></returns>
        /// <remarks>
        /// If the message name is not associated with a known type that was added 
        /// via the <see cref="Add"/> method, the application domain will be scanned
        /// for a type with a matching data contract and the result (if found)
        /// will be added to the known types collection.
        /// </remarks>
        public Type GetTypeForName(MessageName messageName)
        {
            return _typesByMessageName.GetOrAdd(messageName, FindTypeBySchemaTypeName);
        }

        private static MessageName GetSchemaTypeName(Type type)
        {
            var dataContractExporter = new XsdDataContractExporter();
            return dataContractExporter.GetSchemaTypeName(type).ToString();
        }

        private static Type FindTypeBySchemaTypeName(MessageName messageName)
        {
            // Search through all of the types in the current app domain for
            // a type whose schema type name is equal to the message name
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.GetCustomAttributes(typeof(DataContractAttribute), false).Any() 
                    && GetSchemaTypeName(type) == messageName);
        }
    }
}
