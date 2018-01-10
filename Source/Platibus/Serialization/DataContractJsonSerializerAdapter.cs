using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Platibus.Serialization
{
    /// <summary>
    /// An <see cref="ISerializer"/> implementation based on the .NET framework
    /// <see cref="DataContractJsonSerializer"/>
    /// </summary>
    public class DataContractJsonSerializerAdapter : ISerializer
    {
        private readonly DataContractJsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new <see cref="DataContractJsonSerializerAdapter"/> with default settings
        /// </summary>
        public DataContractJsonSerializerAdapter()
        {
            _settings = new DataContractJsonSerializerSettings
            {
                DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK")
                {
                    DateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal
                }
            };
        }

        /// <summary>
        /// Initializes a new <see cref="DataContractJsonSerializerAdapter"/> with the
        /// specified <paramref name="settings"/>
        /// </summary>
        /// <param name="settings">The customized serializer settings</param>
        public DataContractJsonSerializerAdapter(DataContractJsonSerializerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Serializes an object into a string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>Returns the serialized object string</returns>
        public string Serialize(object obj)
        {
            if (obj == null) return null;
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(obj.GetType(), _settings);
                serializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
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
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                var serializer = new DataContractJsonSerializer(type, _settings);
                return serializer.ReadObject(stream);
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