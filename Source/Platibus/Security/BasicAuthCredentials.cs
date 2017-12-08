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

namespace Platibus.Security
{
    /// <inheritdoc cref="IEndpointCredentials"/>
    /// <inheritdoc cref="IEquatable{T}"/>
    /// <summary>
    /// Endpoint crentials consisting of a basic username and password
    /// </summary>
    public sealed class BasicAuthCredentials : IEndpointCredentials, IEquatable<BasicAuthCredentials>
    {
        /// <summary>
        /// The username used to authenticate
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// The password used to authenticate
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Initializes a new <see cref="BasicAuthCredentials"/> with the specified
        /// <paramref name="username"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="username">The username used to authenticate</param>
        /// <param name="password">The password used to authenticate</param>
        public BasicAuthCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        void IEndpointCredentials.Accept(IEndpointCredentialsVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public bool Equals(BasicAuthCredentials other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Username, other.Username) && string.Equals(Password, other.Password);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return ReferenceEquals(this, obj) || Equals(obj as BasicAuthCredentials);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Username != null ? Username.GetHashCode() : 0) * 397) ^ (Password != null ? Password.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Determines equality of two <see cref="BasicAuthCredentials"/> objects based on
        /// their respective values rather than reference
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the left and right operands contain the same
        /// values, <c>false</c> otherwise</returns>
        public static bool operator ==(BasicAuthCredentials left, BasicAuthCredentials right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines inequality of two <see cref="BasicAuthCredentials"/> objects based on
        /// their respective values rather than reference
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the left and right operands contain different
        /// values, <c>false</c> otherwise</returns>
        public static bool operator !=(BasicAuthCredentials left, BasicAuthCredentials right)
        {
            return !Equals(left, right);
        }
    }
}