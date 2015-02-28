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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Pluribus.Security
{
    /// <summary>
    /// Used to capture sender principal information for authorization checks
    /// in a way that can be serialized and stored with a queued message.
    /// </summary>
    [Serializable]
    public class SenderPrincipal : IPrincipal, ISerializable
    {
        private readonly SenderIdentity _identity;
        private readonly SenderRole[] _roles;

        public IIdentity Identity { get { return _identity; } }

        public SenderPrincipal(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (principal.Identity != null)
            {
                _identity = new SenderIdentity(principal.Identity);
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

        protected SenderPrincipal(SerializationInfo info, StreamingContext context)
        {
            _identity = (SenderIdentity)info.GetValue("identity", typeof(SenderIdentity));
            _roles = (SenderRole[])info.GetValue("roles", typeof(SenderRole[]));
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("identity", _identity);
            info.AddValue("roles", _roles);
        }

        public bool IsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            return _roles.Contains(new SenderRole(role));
        }
    }
}
