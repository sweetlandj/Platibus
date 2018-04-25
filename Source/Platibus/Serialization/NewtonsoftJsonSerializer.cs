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
using Newtonsoft.Json;

namespace Platibus.Serialization
{
	/// <inheritdoc />
	/// <summary>
	/// An <see cref="T:Platibus.Serialization.ISerializer" /> implementation based on the Newtonsoft JSON.Net library
	/// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

		/// <inheritdoc />
		/// <summary>
		/// Initializes a new <see cref="T:Platibus.Serialization.NewtonsoftJsonSerializer" /> with recommended
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
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

		/// <inheritdoc />
		public string Serialize(object obj)
		{
		    if (obj == null) return null;
            return JsonConvert.SerializeObject(obj, _settings);
        }

		/// <inheritdoc />
		public object Deserialize(string str, Type type)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            return JsonConvert.DeserializeObject(str, type, _settings);
        }

		/// <inheritdoc />
		public T Deserialize<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return default(T);
            return (T) Deserialize(str, typeof (T));
        }
    }
}