
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
