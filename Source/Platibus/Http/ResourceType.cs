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

namespace Platibus.Http
{
    /// <summary>
    /// A type of HTTP resource with which controllers can be associated in order to
    /// route HTTP resource requests
    /// </summary>
    [DebuggerDisplay("{_value,nq}")]
    public class ResourceType : IEquatable<ResourceType>
    {
        private readonly string _value;

        /// <summary>
        /// Initializes a <see cref="ResourceType"/> with the specified string representation
        /// </summary>
        /// <param name="value">The string representation of the resource type</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is
        /// <c>null</c> or whitespace</exception>
        public ResourceType(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _value = value.Trim();
        }

        /// <summary>
        /// Determines whether another <paramref name="resourceType"/> is equal to this one
        /// </summary>
        /// <param name="resourceType">The other resource type</param>
        /// <returns>Returns <c>true</c> if the other <paramref name="resourceType"/> is equal to
        /// this on; <c>false</c> otherwise</returns>
        public bool Equals(ResourceType resourceType)
        {
            if (ReferenceEquals(null, resourceType)) return false;
            return string.Equals(_value, resourceType._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the string representation of this HTTP resource type
        /// </summary>
        /// <returns>Returns the string representation of this HTTP resource type</returns>
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
            return Equals(obj as ResourceType);
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
        /// Overloaded <c>==</c> operator that determines whether two HTTP resource types are equal
        /// based on value rather than reference
        /// </summary>
        /// <param name="left">The resource type on the left side of the operator</param>
        /// <param name="right">The resource type on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the resource types are equal; <c>false</c> otherwise</returns>
        public static bool operator ==(ResourceType left, ResourceType right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded <c>!=</c> operator that determines whether two HTTP resource types are unequal
        /// based on value rather than reference
        /// </summary>
        /// <param name="left">The resource type on the left side of the operator</param>
        /// <param name="right">The resource type on the right side of the operator</param>
        /// <returns>Returns <c>true</c> if the resource types are unequal; <c>false</c> otherwise</returns>
        public static bool operator !=(ResourceType left, ResourceType right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a string value into a <see cref="ResourceType"/>
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/>is <c>null</c> or whitespace;
        /// otherwise returns a new <see cref="ResourceType"/> instance corresponding to the 
        /// string value</returns>
        public static implicit operator ResourceType(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new ResourceType(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ResourceType"/> to a string
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <returns>Returns <c>null</c> if <paramref name="resourceType"/> is <c>null</c>;
        /// otherwise returns the string representation of the resource type</returns>
        public static implicit operator string(ResourceType resourceType)
        {
            return resourceType == null ? null : resourceType._value;
        }
    }
}