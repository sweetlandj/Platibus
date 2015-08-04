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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platibus.Serialization
{
    public class DefaultSerializationService : ISerializationService, IEnumerable<KeyValuePair<string, ISerializer>>
    {
        private readonly IDictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

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

        public IEnumerator<KeyValuePair<string, ISerializer>> GetEnumerator()
        {
            return _serializers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        protected string NormalizeContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentNullException("contentType");
            return contentType.Split(';').First().Trim().ToLower();
        }

        public void Add(string contentType, ISerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentNullException("contentType");
            if (serializer == null) throw new ArgumentNullException("serializer");
            var key = NormalizeContentType(contentType);
            _serializers[key] = serializer;
        }

        public void Add(KeyValuePair<string, ISerializer> contentTypeSerializer)
        {
            Add(contentTypeSerializer.Key, contentTypeSerializer.Value);
        }
    }
}