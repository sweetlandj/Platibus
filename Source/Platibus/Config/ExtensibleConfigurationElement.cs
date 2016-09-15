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
using System.Configuration;

namespace Platibus.Config
{
    /// <summary>
    /// An abstract base class for configuration elements designed to allow,
    /// capture, and record properties that are not explicitly defined.  These
    /// are used to gather additional provider-specific configuration details.
    /// </summary>
    /// <seealso cref="JournalingElement"/>
    /// <seealso cref="QueueingElement"/>
    /// <seealso cref="SubscriptionTrackingElement"/>
    public abstract class ExtensibleConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as an object (no casting). 
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the property value as an object if it exists;
        /// returns <c>null</c> otherwise.</returns>
        public object GetObject(string name)
        {
            return !Properties.Contains(name) ? null : this[name];
        }

        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as a string.
        /// </summary>
        /// <remarks>
        /// If the property value cannot be cast to a string, then the
        /// result of <seealso cref="object.ToString"/> method is returned.
        /// </remarks>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the property value as a string if it exists;
        /// returns <c>null</c> otherwise.</returns>
        public string GetString(string name)
        {
            var val = GetObject(name);
            if (val == null) return null;

            var strVal = val as string ?? val.ToString();
            return strVal;
        }

        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as a Uri.
        /// </summary>
        /// <remarks>
        /// If the property value cannot be cast to a Uri, then a new Uri
        /// object is initialized with the result of the object's 
        /// <seealso cref="object.ToString"/> method and returned.
        /// </remarks>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the property value as a Uri if it exists;
        /// returns <c>null</c> otherwise.</returns>
        /// <exception cref="UriFormatException">Thrown if the result of the
        /// object's <seealso cref="object.ToString"/> method is not a valid
        /// URI.</exception>
        public Uri GetUri(string name)
        {
            var val = GetObject(name);
            if (val == null) return null;

            var uri = val as Uri;
            if (uri != null) return uri;

            var strVal = val as string ?? val.ToString();
            return new Uri(strVal);
        }

        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as an int.
        /// </summary>
        /// <remarks>
        /// If the property value cannot be cast to an int then the
        /// <see cref="Convert.ToInt32(object)"/> method is used to try
        /// to convert to an integer.
        /// </remarks>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the property value as an int</returns>
        /// <exception cref="InvalidCastException">If the property value
        /// cannot be cast or converted to an int</exception>
        /// <exception cref="FormatException">If the property value
        /// cannot be cast or converted to an int</exception>
        public int GetInt(string name)
        {
            var val = GetObject(name);
            if (val == null) return default(int);

            if (val is int)
            {
                return (int) val;
            }
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as a bool.
        /// </summary>
        /// <remarks>
        /// If the property value cannot be cast to a bool then the
        /// <see cref="Convert.ToBoolean(object)"/> method is used to try
        /// to convert to an bool.
        /// </remarks>
        /// <param name="name">The name of the property</param>
        /// <returns>Returns the property value as a bool</returns>
        /// <exception cref="InvalidCastException">If the property value
        /// cannot be cast or converted to a bool</exception>
        /// <exception cref="FormatException">If the property value
        /// cannot be cast or converted to a bool</exception>
        public bool GetBool(string name)
        {
            var val = GetObject(name);
            if (val == null) return default(bool);

            if (val is bool)
            {
                return (bool) val;
            }
            return Convert.ToBoolean(val);
        }

        /// <summary>
        /// Returns the property with the specified <paramref name="name"/>
        /// as a <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">The enum type</typeparam>
        /// <remarks>
        /// If the property value cannot be cast to <typeparamref name="TEnum"/> 
        /// then the result of the object's <seealso cref="object.ToString"/> 
        /// method is passed to the <see cref="Enum.Parse(System.Type,string)"/>
        /// method and the result is returned.
        /// </remarks>
        /// <param name="name">The name of the property</param>
        /// <returns>
        /// Returns the property value as a <typeparamref name="TEnum"/>
        /// </returns>
        /// <exception cref="FormatException">If the property value
        /// cannot be cast or parsed to <typeparamref name="TEnum"/></exception>
        public TEnum GetEnum<TEnum>(string name) where TEnum : struct
        {
            var val = GetObject(name);
            if (val == null) return default(TEnum);

            if (val is TEnum)
            {
                return (TEnum) val;
            }
            return (TEnum) Enum.Parse(typeof (TEnum), val.ToString(), false);
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        /// <param name="name">The name of the unrecognized attribute.</param><param name="value">The value of the unrecognized attribute.</param>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (!Properties.Contains(name))
            {
                Properties.Add(new ConfigurationProperty(name, typeof (string), null));
            }

            this[name] = value;
            return true;
        }
    }
}