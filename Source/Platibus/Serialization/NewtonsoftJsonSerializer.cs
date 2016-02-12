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
using Newtonsoft.Json;

namespace Platibus.Serialization
{
	/// <summary>
	/// An <see cref="ISerializer"/> implementation based on the Newtonsoft JSON.Net library
	/// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

		/// <summary>
		/// Initializes a new <see cref="NewtonsoftJsonSerializer"/> with recommended
		/// serializer settings
		/// </summary>
        public NewtonsoftJsonSerializer() : this(null)
        {
        }

		/// <summary>
		/// Initializes a new <see cref="NewtonsoftJsonSerializer"/> with the
		/// specified <see cref="JsonSerializerSettings"/>
		/// </summary>
		/// <param name="settings">The JSON serializer settings to use</param>
        public NewtonsoftJsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.None,
                MaxDepth = 10,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

		/// <summary>
		/// Serializes an object into a string
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <returns>Returns the serialized object string</returns>
		public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

		/// <summary>
		/// Deserializes a string into an object of the specified <paramref name="type"/>
		/// </summary>
		/// <param name="str">The serialized object string</param>
		/// <param name="type">The type of object</param>
		/// <returns>Returns a deserialized object of the specified type</returns>
		public object Deserialize(string str, Type type)
        {
            return JsonConvert.DeserializeObject(str, type, _settings);
        }

		/// <summary>
		/// Deserializes a string into an object of the specified type <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="str">The serialized object string</param>
		/// <returns>Returns a deserialized object of type <typeparamref name="T"/></returns>
		public T Deserialize<T>(string str)
        {
            return (T) Deserialize(str, typeof (T));
        }
    }
}