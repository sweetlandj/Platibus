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
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;

namespace Platibus.Security
{
    /// <summary>
    /// A serializable representation of a sender principal role
    /// </summary>
    [Serializable]
    [Obsolete("Use MessageSecurityToken")]
    public class SenderRole : IEquatable<SenderRole>, ISerializable
    {
        /// <summary>
        /// The role name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new <see cref="SenderRole"/> with the specified <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the role</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is
        /// <c>null</c> or whitespace</exception>
        public SenderRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name.Trim();
        }

        /// <summary>
        /// Initializes a new <see cref="SenderRole"/> with the specified role or group 
        /// membership <paramref name="claim"/>
        /// </summary>
        /// <param name="claim">The role or group membership claim</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="claim"/> is
        /// <c>null</c></exception>
        public SenderRole(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            Name = claim.Value.Trim();
        }

        /// <summary>
        /// Initializes a serialized <see cref="SenderRole"/> from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected SenderRole(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
        }

        /// <summary>
        /// Returns a string representation of the role (i.e. the role name)
        /// </summary>
        /// <returns>Returns a string representation of the role (i.e. the role name)</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param><param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
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
            return Name.ToLower().GetHashCode();
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
            return Equals(obj as SenderRole);
        }

        /// <summary>
        /// Determines whether another <paramref name="senderRole"/> is equal to this one
        /// </summary>
        /// <param name="senderRole">The other sender role</param>
        /// <returns>Returns <c>true</c> if the other sender role is equal to this one;
        /// <c>false</c> otherwise</returns>
        public bool Equals(SenderRole senderRole)
        {
            if (ReferenceEquals(null, senderRole)) return false;
            if (ReferenceEquals(this, senderRole)) return true;
            return Name.Equals(senderRole.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Implicitly converts a string role <paramref name="name"/> into a <see cref="SenderRole"/>
        /// </summary>
        /// <param name="name">The role name</param>
        /// <returns>Returns <c>null</c> if <paramref name="name"/> is <c>null</c> or
        /// whitespace; otherwise returns a new <see cref="SenderRole"/> for the role name</returns>
        public static implicit operator SenderRole(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : new SenderRole(name);
        }

        /// <summary>
        /// Implicitly converts a <see cref="SenderRole"/> into its string representation (i.e.
        /// the role name)
        /// </summary>
        /// <param name="role">The sender role</param>
        /// <returns>Returns <c>null</c> if <paramref name="role"/> is <c>null</c>;
        /// otherwise returns the string representation of the sender role</returns>
        public static implicit operator string(SenderRole role)
        {
            return role == null ? null : role.Name;
        }
    }
}