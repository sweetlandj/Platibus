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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platibus.AspNetCore
{
    internal class ContentType
    {
        private const string CharsetAttributeName = "charset";

        public string Type { get; }

        public string Subtype { get; }

        public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public Encoding CharsetEncoding
        {
            get
            {
                if (Parameters.TryGetValue(CharsetAttributeName, out string charset))
                {
                    try
                    {
                        return Encoding.GetEncoding(charset);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }
                return Encoding.UTF8;
            }
            set
            {
                if (value != null)
                {
                    Parameters[CharsetAttributeName] = value.WebName;
                }
                else
                {
                    Parameters.Remove(CharsetAttributeName);
                }
            }
        }

        public ContentType(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException();
            var parts = value.Split(';')
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => str.Trim())
                .ToList();

            var typeAndSubtype = parts.First().Split('/').ToList();
            Type = typeAndSubtype.First().Trim();
            Subtype = typeAndSubtype.Skip(1).Select(str => str.Trim()).FirstOrDefault();

            var parameters = parts
                .Skip(1)
                .Select(ParseParameter)
                .Where(parameter => parameter.Value != null);

            foreach (var keyValuePair in parameters)
            {
                Parameters[keyValuePair.Key.ToLower()] = keyValuePair.Value;
            }
        }

        public override string ToString()
        {
            var value = Type;
            if (!string.IsNullOrWhiteSpace(Subtype)) value += "/" + Subtype;
            foreach (var parameter in Parameters)
            {
                value += "; " + parameter.Key + "=" + parameter.Value;
            }
            return value;
        }

        private static KeyValuePair<string, string> ParseParameter(string parameter)
        {
            var attributeAndValue = parameter.Split('=')
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => str.Trim())
                .ToList();

            var attriubute = attributeAndValue.FirstOrDefault();
            var value = attributeAndValue.Skip(1).FirstOrDefault();
            return new KeyValuePair<string, string>(attriubute, value);
        }

        public static implicit operator ContentType(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new ContentType(value);
        }

        public static implicit operator string(ContentType contentType)
        {
            return contentType == null ? null : contentType.ToString();
        }
    }
}
