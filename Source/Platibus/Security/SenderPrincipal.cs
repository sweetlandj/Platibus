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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Principal;

namespace Platibus.Security
{
    /// <summary>
    /// A serializable representation of a sender principal
    /// </summary>
    /// <remarks>
    /// Ensures that the sender principal can be deserialized in different security
    /// contexts
    /// </remarks>
    [Serializable]
    public class SenderPrincipal : IPrincipal, ISerializable
    {
        private readonly SenderIdentity _identity;
        private readonly SenderRole[] _roles;

        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Security.Principal.IIdentity"/> object associated with the current principal.
        /// </returns>
        public IIdentity Identity
        {
            get { return _identity; }
        }

        /// <summary>
        /// Initializes a new <see cref="SenderPrincipal"/> based on the specifieed 
        /// <paramref name="principal"/>
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="principal"/> is
        /// <c>null</c></exception>
        public SenderPrincipal(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (principal.Identity != null)
            {
                _identity = SenderIdentity.From(principal.Identity);
            }

            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                var roleClaimType = ClaimTypes.Role;
                var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    roleClaimType = claimsIdentity.RoleClaimType;
                }

                _roles = claimsPrincipal.Claims
                    .Where(claim => claim.Type == roleClaimType)
                    .Select(claim => new SenderRole(claim))
                    .Distinct()
                    .ToArray();
            }
            else
            {
                _roles = new SenderRole[0];
            }
        }

        /// <summary>
        /// Initializes a serialized <see cref="SenderPrincipal"/> from a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected SenderPrincipal(SerializationInfo info, StreamingContext context)
        {
            _identity = (SenderIdentity) info.GetValue("Identity", typeof (SenderIdentity));
            _roles = (SenderRole[]) info.GetValue("Roles", typeof (SenderRole[]));
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data. </param><param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization. </param><exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Identity", _identity);
            info.AddValue("Roles", _roles);
        }

        /// <summary>
        /// Determines whether the current principal belongs to the specified role.
        /// </summary>
        /// <returns>
        /// true if the current principal is a member of the specified role; otherwise, false.
        /// </returns>
        /// <param name="role">The name of the role for which to check membership. </param>
        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;

            var ntAccount = new NTAccount(role);
            try
            {
                var sid = ntAccount.Translate(typeof (SecurityIdentifier)) as SecurityIdentifier;
                if (sid != null)
                {
                    if (_roles.Contains(new SenderRole(sid.ToString())))
                    {
                        return true;
                    }
                }
            }
            catch (IdentityNotMappedException)
            {
            }

            return _roles.Contains(new SenderRole(role));
        }

        /// <summary>
        /// Returns a <see cref="SenderPrincipal"/> derived from the specified <paramref name="principal"/>
        /// </summary>
        /// <remarks>
        /// If the supplied <paramref name="principal"/> is a <see cref="SenderPrincipal"/> then it will
        /// be returned unmodified.
        /// </remarks>
        /// <param name="principal">The principal from which the <see cref="SenderPrincipal"/> should be
        /// derived</param>
        /// <returns>Returns a <see cref="SenderPrincipal"/> derived from the supplied <paramref name="principal"/>
        /// </returns>
        public static SenderPrincipal From(IPrincipal principal)
        {
            var senderPrincipal = principal as SenderPrincipal;
            if (senderPrincipal == null && principal != null)
            {
                senderPrincipal = new SenderPrincipal(principal);
            }
            return senderPrincipal;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _identity.Name;
        }
    }
}