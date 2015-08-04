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
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Platibus.Serialization
{
    public class DataContractSerializerAdapter : ISerializer
    {
        public string Serialize(object obj)
        {
            var stringWriter = new StringWriter();
            using (var xmlWriter = new XmlTextWriter(stringWriter))
            {
                var serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(xmlWriter, obj);
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        public object Deserialize(string str, Type type)
        {
            var stringReader = new StringReader(str);
            using (var xmlReader = new XmlTextReader(stringReader))
            {
                var serializer = new DataContractSerializer(type);
                return serializer.ReadObject(xmlReader);
            }
        }

        public T Deserialize<T>(string str)
        {
            return (T) Deserialize(str, typeof (T));
        }
    }
}