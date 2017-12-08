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
using System.Text.RegularExpressions;

namespace Platibus.Config
{
    /// <summary>
    /// An <see cref="IMessageSpecification"/> implementation that matches
    /// messages based on the value of the <see cref="HeaderName.MessageName"/> 
    /// header.
    /// </summary>
    public class MessageNamePatternSpecification : IMessageSpecification, IEquatable<MessageNamePatternSpecification>
    {
        /// <summary>
        /// The regular expression used to match message names
        /// </summary>
        public Regex NameRegex { get; }

        /// <summary>
        /// Initializes a new <see cref="MessageNamePatternSpecification"/>
        /// that will match message names using the specified regular
        /// expression
        /// </summary>
        /// <param name="namePattern">A regular expression used to match
        /// message names</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="namePattern"/>
        /// is <c>null</c> or whitespace</exception>
        public MessageNamePatternSpecification(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern)) throw new ArgumentNullException(nameof(namePattern));
            NameRegex = new Regex(namePattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// Indicates whether the specification is satisfied by the 
        /// supplied <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message in question</param>
        /// <returns>Returns <c>true</c> if the specification is
        /// satisfied by the message; <c>false</c> otherwise.</returns>
        public bool IsSatisfiedBy(Message message)
        {
            var messageName = (string)message.Headers.MessageName ?? "";
            return (NameRegex == null || NameRegex.IsMatch(messageName));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(MessageNamePatternSpecification other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(NameRegex.ToString(), other.NameRegex.ToString());
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == GetType() && Equals((MessageNamePatternSpecification) obj);
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
            return (NameRegex != null ? NameRegex.GetHashCode() : 0);
        }

        /// <summary>
        /// Indicates whether two message name specifications are equivalent by
        /// virtue of having identical regular expressions.
        /// </summary>
        /// <param name="left">The message name specification on the left hand side of the operator</param>
        /// <param name="right">The message name specification on the right hand side of the operator</param>
        /// <returns>Returns <c>true</c> if the two message name specifications are equal; <c>false</c> if
        /// they are not</returns>
        public static bool operator ==(MessageNamePatternSpecification left, MessageNamePatternSpecification right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Indicates whether two message name specifications are not equivalent by
        /// virtue of having different regular expressions.
        /// </summary>
        /// <param name="left">The message name specification on the left hand side of the operator</param>
        /// <param name="right">The message name specification on the right hand side of the operator</param>
        /// <returns>Returns <c>true</c> if the two message name specifications are not equal; <c>false</c> if
        /// they are equal</returns>
        public static bool operator !=(MessageNamePatternSpecification left, MessageNamePatternSpecification right)
        {
            return !Equals(left, right);
        }
    }
}