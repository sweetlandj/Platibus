// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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
using System.Configuration;

namespace Platibus.Config
{
    public abstract class ExtensibleConfigurationElement : ConfigurationElement
    {
        public object GetObject(string name)
        {
            if (!Properties.Contains(name)) return null;
            return this[name];
        }

        public string GetString(string name)
        {
            var val = GetObject(name);
            if (val == null) return null;

            var strVal = val as string;
            if (strVal == null)
            {
                strVal = val.ToString();
            }
            return strVal;
        }

        public int GetInt(string name)
        {
            var val = GetObject(name);
            if (val == null) return default(int);

            if (val is int)
            {
                return (int)val;
            }
            return Convert.ToInt32(val);
        }

        public bool GetBool(string name)
        {
            var val = GetObject(name);
            if (val == null) return default(bool);

            if (val is bool)
            {
                return (bool)val;
            }
            return Convert.ToBoolean(val);
        }

        public TEnum GetEnum<TEnum>(string name) where TEnum : struct
        {
            var val = GetObject(name);
            if (val == null) return default(TEnum);

            if (val is TEnum)
            {
                return (TEnum)val;
            }
            return (TEnum)Enum.Parse(typeof(TEnum), val.ToString(), false);
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (!Properties.Contains(name))
            {
                Properties.Add(new ConfigurationProperty(name, typeof(string), null));
            }

            this[name] = value;
            return true;
        }
    }
}
