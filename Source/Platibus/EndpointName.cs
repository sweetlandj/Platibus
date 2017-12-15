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
using System.Diagnostics;

namespace Platibus
{
    /// <summary>
    /// An amplified value type that provides equality semantics for endpoint names
    /// </summary>
    [DebuggerDisplay("{" + nameof(_value) + ",nq}")]
    public class EndpointName : IEquatable<EndpointName>
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a new <see cref="EndpointName"/> with the specified <paramref name="value"/>
        /// </summary>
        /// <param name="value">The endpoint name as a string</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is
        /// <c>null</c> or whitespace</exception>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> contains invalid
        /// characters</exception>
        /// <remarks>
        /// Endpoint names may not contain the character ':'
        /// </remarks>
        public EndpointName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            _value = value.Trim();
        }

        /// <summary>
        /// Determines whether another <paramref name="endpointName"/> is equal to this one
        /// </summary>
        /// <param name="endpointName">The other endpoint name</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="endpointName"/> is equal
        /// to this one; <c>fale</c> otherwise</returns>
        public bool Equals(EndpointName endpointName)
        {
            if (ReferenceEquals(null, endpointName)) return false;
            return string.Equals(_value, endpointName._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return _value;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            return Equals(obj as EndpointName);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return _value.ToLowerInvariant().GetHashCode();
        }

        /// <summary>
        /// Overloaded equality operator used to determine whether two <see cref="EndpointName"/>
        /// objects are equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two endpoint names are equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator ==(EndpointName left, EndpointName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded inequality operator used to determine whether two <see cref="EndpointName"/>
        /// objects are not equal
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two endpoint names are not equal; <c>false</c>
        /// otherwise</returns>
        public static bool operator !=(EndpointName left, EndpointName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string into a <see cref="EndpointName"/>
        /// </summary>
        /// <param name="value">The endpoint name as a string</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c> or
        /// whitespace; otherwise returns a new <see cref="EndpointName"/> instance
        /// with the specified <paramref name="value"/></returns>
        public static implicit operator EndpointName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new EndpointName(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="EndpointName"/> object to a string
        /// </summary>
        /// <param name="endpointName">The endpoint name object</param>
        /// <returns>Returns <c>null</c> if <paramref name="endpointName"/> is <c>null</c>;
        /// otherwise returns the value of the <paramref name="endpointName"/> object</returns>
        public static implicit operator string(EndpointName endpointName)
        {
            return endpointName?._value;
        }
    }
}