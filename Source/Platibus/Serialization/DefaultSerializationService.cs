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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Platibus.Serialization
{
    /// <summary>
    /// A basic <see cref="ISerializationService"/> implementation that uses a
    /// dictionary to match MIME content types to registered <see cref="ISerializer"/>
    /// implementations
    /// </summary>
    public class DefaultSerializationService : ISerializationService, IEnumerable<KeyValuePair<string, ISerializer>>
    {
        private readonly IDictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        /// <summary>
        /// Initializes a new <see cref="DefaultSerializationService"/>
        /// </summary>
        /// <param name="useDataContractSerializer">Whether to use the .NET framework
        /// data contract serializer (<c>true</c>) or the XML serializer (<c>false</c>)
        /// for "application/xml" and "text/xml" content types</param>
        /// <remarks>
        /// Data contract serialization should only be used if there is a reasonable
        /// expectation that all types that are serialized are correctly annotated
        /// with <see cref="DataContractAttribute"/>
        /// </remarks>
        public DefaultSerializationService(bool useDataContractSerializer = false)
        {
            var applicationJson = NormalizeContentType("application/json");
            var defaultJsonSerializer = new NewtonsoftJsonSerializer();
            _serializers[applicationJson] = defaultJsonSerializer;

            var applicationXml = NormalizeContentType("application/xml");
            var textXml = NormalizeContentType("text/xml");
            var defaultXmlSerializer = useDataContractSerializer 
                ? new DataContractSerializerAdapter()
                : new XmlSerializerAdapter() as ISerializer;

            _serializers[applicationXml] = defaultXmlSerializer;
            _serializers[textXml] = defaultXmlSerializer;

            var textPlain = NormalizeContentType("text/plain");
            _serializers[textPlain] = new StringSerializer();

            var applicationOctetStream = NormalizeContentType("application/octet-stream");
            _serializers[applicationOctetStream] = new Base64ObjectSerializer();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<string, ISerializer>> GetEnumerator()
        {
            return _serializers.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the most appropriate serializer for the specified content type
        /// </summary>
        /// <param name="forContentType">The MIME content type</param>
        /// <returns>Returns a serializer for the specified content type</returns>
        public ISerializer GetSerializer(string forContentType)
        {
            ISerializer serializer;
            var key = NormalizeContentType(forContentType);
            if (!_serializers.TryGetValue(key, out serializer) || serializer == null)
            {
                throw new SerializerNotFoundException(forContentType);
            }
            return serializer;
        }

        /// <summary>
        /// Ensures that content type strings are converted to the same case, trimmed,
        /// etc. so that they can be compared and used as dictionary keys
        /// </summary>
        /// <param name="contentType">The content type to normalize</param>
        /// <returns>A normalized content type string</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentType"/>
        /// is <c>null</c> or whitespace</exception>
        protected string NormalizeContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentNullException("contentType");
            return contentType.Split(';').First().Trim().ToLower();
        }

        /// <summary>
        /// Registers a new serializer for a content type
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <param name="serializer">The serializer</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="contentType"/>
        /// or <paramref name="serializer"/> is <c>null</c> or whitespace</exception>
        /// <remarks>
        /// Overwrites any previously registered serializers for the specified content type
        /// </remarks>
        public void Add(string contentType, ISerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentNullException("contentType");
            if (serializer == null) throw new ArgumentNullException("serializer");
            var key = NormalizeContentType(contentType);
            _serializers[key] = serializer;
        }

        /// <summary>
        /// Registers a new serializer for a content type
        /// </summary>
        /// <param name="contentTypeSerializer">A key-value pair consisting of the content
        /// type and te serializer</param>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <paramref name="contentTypeSerializer"/> is <c>null</c></exception>
        /// <remarks>
        /// Overwrites any previously registered serializers for the specified content type.
        /// This method exists to support the list initializer syntax.
        /// </remarks>
        public void Add(KeyValuePair<string, ISerializer> contentTypeSerializer)
        {
            Add(contentTypeSerializer.Key, contentTypeSerializer.Value);
        }
    }
}