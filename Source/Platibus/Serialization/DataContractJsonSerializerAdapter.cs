using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace Platibus.Serialization
{
    /// <summary>
    /// An <see cref="ISerializer"/> implementation based on the .NET framework
    /// <see cref="DataContractJsonSerializer"/>
    /// </summary>
    public class DataContractJsonSerializerAdapter : ISerializer
    {
        /// <summary>
        /// Serializes an object into a string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>Returns the serialized object string</returns>
        public string Serialize(object obj)
        {
            if (obj == null) return null;
            var stringWriter = new StringWriter();
            using (var xmlWriter = new XmlTextWriter(stringWriter))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(xmlWriter, obj);
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Deserializes a string into an object of the specified <paramref name="type"/>
        /// </summary>
        /// <param name="str">The serialized object string</param>
        /// <param name="type">The type of object</param>
        /// <returns>Returns a deserialized object of the specified type</returns>
        public object Deserialize(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            var stringReader = new StringReader(str);
            using (var xmlReader = new XmlTextReader(stringReader))
            {
                var serializer = new DataContractJsonSerializer(type);
                return serializer.ReadObject(xmlReader);
            }
        }

        /// <summary>
        /// Deserializes a string into an object of the specified type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="str">The serialized object string</param>
        /// <returns>Returns a deserialized object of type <typeparamref name="T"/></returns>
        public T Deserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return default(T);
            return (T) Deserialize(str, typeof (T));
        }
    }
}