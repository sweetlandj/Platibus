using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Platibus.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// An <see cref="T:Platibus.Serialization.ISerializer" /> implementation based on the .NET framework
    /// <see cref="T:System.Runtime.Serialization.Json.DataContractJsonSerializer" />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public object Deserialize(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                var serializer = new DataContractJsonSerializer(type, _settings);
                return serializer.ReadObject(stream);
            }
        }

        /// <inheritdoc />
        public T Deserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return default(T);
            return (T) Deserialize(str, typeof (T));
        }
    }
}