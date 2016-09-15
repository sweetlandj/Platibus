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
using System.Security.Permissions;
using System.Security.Principal;

namespace Platibus.Security
{
    /// <summary>
    /// A serializable representation of a sender identity
    /// </summary>
    /// <remarks>
    /// Ensures that the sender principal can be deserialized in different security
    /// contexts
    /// </remarks>
    [Serializable]
    public class SenderIdentity : IIdentity, ISerializable
    {
        private readonly string _name;
        private readonly string _authenticationType;
        private readonly bool _isAuthenticated;

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>
        /// true if the user was authenticated; otherwise, false.
        /// </returns>
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Initializes a new <see cref="SenderIdentity"/> based on the specified
        /// <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The sender identity</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="identity"/> is
        /// <c>null</c></exception>
        public SenderIdentity(IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            _name = identity.Name;
            _authenticationType = identity.AuthenticationType;
            _isAuthenticated = identity.IsAuthenticated;
        }

        /// <summary>
        /// Initializes a serializes <see cref="SenderIdentity"/> from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected SenderIdentity(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("Name");
            _authenticationType = info.GetString("AuthenticationType");
            _isAuthenticated = info.GetBoolean("IsAuthenticated");
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param><param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", _name);
            info.AddValue("AuthenticationType", _authenticationType);
            info.AddValue("IsAuthenticated", _isAuthenticated);
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
            return _name;
        }

        /// <summary>
        /// Returns a <see cref="SenderIdentity"/> derived from the specified <paramref name="identity"/>
        /// </summary>
        /// <remarks>
        /// If the supplied <paramref name="identity"/> is a <see cref="SenderIdentity"/> then it will
        /// be returned unmodified.
        /// </remarks>
        /// <param name="identity">The identity from which the <see cref="SenderIdentity"/> should be
        /// derived</param>
        /// <returns>Returns a <see cref="SenderIdentity"/> derived from the supplied <paramref name="identity"/>
        /// </returns>
        public static SenderIdentity From(IIdentity identity)
        {
            var senderIdentity = identity as SenderIdentity;
            if (senderIdentity == null && identity != null)
            {
                senderIdentity = new SenderIdentity(identity);
            }
            return senderIdentity;
        }
    }
}