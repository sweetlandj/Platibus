using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus
{
    [DebuggerDisplay("{_value,nq}")]
    public class MessageDurability
    {
        public static readonly MessageDurability None = "None";
        public static readonly MessageDurability Allowed = "Allowed";
        public static readonly MessageDurability Requested = "Requested";
        public static readonly MessageDurability Default = Allowed;

        private readonly string _value;

        public bool IsAllowed
        {
            get { return Allowed.Equals(this) || Requested.Equals(this); }
        }

        public bool IsRequested
        {
            get { return Requested.Equals(this); }
        }

        public MessageDurability(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            _value = value.Trim();
        }

        public bool Equals(MessageDurability messageDurability)
        {
            if (ReferenceEquals(null, messageDurability)) return false;
            return string.Equals(_value, messageDurability._value, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return _value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageDurability);
        }

        public override int GetHashCode()
        {
            return _value.ToLowerInvariant().GetHashCode();
        }

        public static bool operator ==(MessageDurability left, MessageDurability right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageDurability left, MessageDurability right)
        {
            return !Equals(left, right);
        }

        public static implicit operator MessageDurability(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new MessageDurability(value);
        }

        public static implicit operator string(MessageDurability messageDurability)
        {
            return messageDurability == null ? null : messageDurability._value;
        }
    }
}
