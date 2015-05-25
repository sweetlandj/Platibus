using System;
using System.Diagnostics;

namespace Platibus
{
    [DebuggerDisplay("{_value,nq}")]
    public struct MessageImportance : IComparable<MessageImportance>
    {
        private const int LowValue = -1;
        private const int NormalValue = 0;
        private const int HighValue = 1;
        private const int CriticalValue = 2;

        /// <summary>
        /// Low importance.  Can be discarded in cases of extreme congestion.
        /// </summary>
        public static readonly MessageImportance Low = new MessageImportance(LowValue);

        /// <summary>
        /// Normal importance.  Lowest priority for which discarding the message is not
        /// an option.  A best effort attempt must be made to process the message, but
        /// the receiver is not required to queue the message or retry processing.
        /// </summary>
        public static readonly MessageImportance Normal = new MessageImportance(NormalValue);

        /// <summary>
        /// High importance.  Messages should be prioritized over messages with <see cref="LowValue"/> 
        /// and <see cref="NormalValue"/> importance and should never be discarded.  A best effort
        /// attempt must be made to process the message, but the receiver is not required to
        /// queue the message or retry processing.
        /// </summary>
        public static readonly MessageImportance High = new MessageImportance(HighValue);

        /// <summary>
        /// Critical importance.  The message must be queued and processed at least once.
        /// </summary>
        public static readonly MessageImportance Critical = new MessageImportance(CriticalValue);

        private readonly int _value;

        public bool AllowsDiscarding
        {
            get { return _value <= LowValue; }
        }

        public bool RequiresQueueing
        {
            get { return _value >= CriticalValue; }
        }

        public MessageImportance(int value)
        {
            _value = value;
        }

        public bool Equals(MessageImportance importance)
        {
            return _value == importance._value;
        }

        public override string ToString()
        {
            return _value.ToString("d");
        }

        public override bool Equals(object obj)
        {
            return obj is MessageImportance && Equals((MessageImportance) obj);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public static bool operator ==(MessageImportance left, MessageImportance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MessageImportance left, MessageImportance right)
        {
            return !Equals(left, right);
        }

        public static implicit operator MessageImportance(int value)
        {
            return new MessageImportance(value);
        }

        public static implicit operator int(MessageImportance importance)
        {
            return importance._value;
        }

        public int CompareTo(MessageImportance other)
        {
            return _value.CompareTo(other._value);
        }
    }
}