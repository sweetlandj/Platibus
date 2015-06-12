
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;

namespace Platibus
{
    public class DataContractMessageNamingService : IMessageNamingService
    {
        private readonly ConcurrentDictionary<Type, MessageName> _messageNamesByType = new ConcurrentDictionary<Type, MessageName>();
        private readonly ConcurrentDictionary<MessageName, Type> _typesByMessageName = new ConcurrentDictionary<MessageName, Type>();

        public void Add(Type messageType)
        {
            var messageName = GetSchemaTypeName(messageType);
            _messageNamesByType.TryAdd(messageType, messageName);
            _typesByMessageName.TryAdd(messageName, messageType);
        }

        public MessageName GetNameForType(Type messageType)
        {
            return _messageNamesByType.GetOrAdd(messageType, GetSchemaTypeName);
        }
        
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
