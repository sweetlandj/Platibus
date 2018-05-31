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
using Platibus.Diagnostics;
using Platibus.Utils;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// A message naming service based on known types annotated with data contracts
    /// </summary>
    /// <seealso cref="T:System.Runtime.Serialization.DataContractAttribute" />
    public class DataContractMessageNamingService : IMessageNamingService
    {
        private readonly IDiagnosticService _diagnosticService;
        private readonly IReflectionService _reflectionService;
        private readonly XsdDataContractExporter _dataContractExporter = new XsdDataContractExporter();
        
        private readonly ConcurrentDictionary<Type, MessageName> _messageNamesByType = new ConcurrentDictionary<Type, MessageName>();
        private readonly ConcurrentDictionary<MessageName, Type> _typesByMessageName = new ConcurrentDictionary<MessageName, Type>();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.DataContractMessageNamingService" />
        /// </summary>
        public DataContractMessageNamingService() : this(null)
        {
            _diagnosticService = DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a new <see cref="DataContractMessageNamingService"/>
        /// </summary>
        /// <param name="diagnosticService">The diagnostic service to which events
        /// related to data contract resolution will be directed</param>
        public DataContractMessageNamingService(IDiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            _reflectionService = new DefaultReflectionService(_diagnosticService);
        }

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

        /// <inheritdoc />
        /// <summary>
        /// Returns the canonical message name for a given type
        /// </summary>
        /// <param name="messageType">The message type</param>
        /// <returns>The canonical name associated with the type</returns>
        /// <remarks>
        /// For types annotated with <see cref="T:System.Runtime.Serialization.DataContractAttribute" />, the
        /// message name will be the schema type name
        /// </remarks>
        public MessageName GetNameForType(Type messageType)
        {
            return _messageNamesByType.GetOrAdd(messageType, GetSchemaTypeName);
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns the type associated with a canonical message name
        /// </summary>
        /// <param name="messageName">The canonical message name</param>
        /// <returns>The type best suited to deserialized content for messages
        /// with the specified <paramref name="messageName" /></returns>
        /// <remarks>
        /// If the message name is not associated with a known type that was added 
        /// via the <see cref="M:Platibus.DataContractMessageNamingService.Add(System.Type)" /> method, the application domain will be scanned
        /// for a type with a matching data contract and the result (if found)
        /// will be added to the known types collection.
        /// </remarks>
        public Type GetTypeForName(MessageName messageName)
        {
            return _typesByMessageName.GetOrAdd(messageName, FindTypeBySchemaTypeName);
        }

        private MessageName GetSchemaTypeName(Type type)
        {
            try
            {
                return _dataContractExporter.GetSchemaTypeName(type).ToString();
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.InvalidDataContract)
                {
                    Detail = $"Error determining schema type name for {type}",
                    Exception = ex
                }.Build());

                // Applly the default namespace convention
                return $"http://schemas.datacontract.org/2004/07/{type.Namespace}:{type.Name}";
            }
        }

        private Type FindTypeBySchemaTypeName(MessageName messageName)
        {
            // Search through all of the types in the current app domain for
            // a type whose schema type name is equal to the message name
            return _reflectionService
                .EnumerateTypes()
                .Where(t => !t.IsGenericTypeDefinition)
                .FirstOrDefault(type => type.Has<DataContractAttribute>() 
                                        && GetSchemaTypeName(type) == messageName);
        }
    }
}
