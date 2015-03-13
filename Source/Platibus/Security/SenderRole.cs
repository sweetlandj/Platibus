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
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Permissions;

namespace Platibus.Security
{
    [Serializable]
    public class SenderRole : IEquatable<SenderRole>, ISerializable
    {
        private readonly string _name;

        public SenderRole(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _name = value.Trim();
        }

        public SenderRole(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException("claim");
            _name = claim.Value.Trim();
        }

        protected SenderRole(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("name");
        }

        public override string ToString()
        {
            return _name;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", _name);
        }

        public override int GetHashCode()
        {
            return _name.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SenderRole);
        }

        public bool Equals(SenderRole senderRole)
        {
            if (ReferenceEquals(null, senderRole)) return false;
            if (ReferenceEquals(this, senderRole)) return true;
            return _name.Equals(senderRole._name, StringComparison.OrdinalIgnoreCase);
        }

        public static implicit operator SenderRole(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : new SenderRole(name);
        }

        public static implicit operator string(SenderRole role)
        {
            return role == null ? null : role._name;
        }
    }
}
