using System;
using System.Diagnostics;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Represents a Unix time value
    /// </summary>
    [DebuggerDisplay("{_value,nq}")]
    public struct UnixTime : IEquatable<UnixTime>
    {
        /// <summary>
        /// The Unix epoch
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns the current Unix time
        /// </summary>
        public static UnixTime Current
        {
            get
            {
                var timestamp = DateTime.UtcNow;
                return new UnixTime(timestamp);
            }
        }

        private readonly double _value;

        /// <summary>
        /// Returns the seconds elapsed since the <see cref="Epoch"/>
        /// </summary>
        public double Value { get { return _value; } }

        /// <summary>
        /// Initializes a new <see cref="UnixTime"/> from the specified .NET <see cref="DateTime"/>
        /// value
        /// </summary>
        /// <param name="timestamp">The .NET date/time value</param>
        public UnixTime(DateTime timestamp)
        {
            var utcTimestamp = timestamp.ToUniversalTime();

            // Round to millisecond precision
            _value = Math.Round((utcTimestamp - Epoch).TotalSeconds, 3);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString("F");
        }

        /// <summary>
        /// Returns the .NET <see cref="DateTime"/> representation of the Unix time value
        /// </summary>
        /// <returns>Returns the .NET <see cref="DateTime"/> representation of the Unix time value</returns>
        public DateTime ToDateTime()
        {
            return Epoch + TimeSpan.FromSeconds(_value);
        }
        
        /// <summary>
        /// Initializes a new <see cref="UnixTime"/> from the specified <paramref name="value"/>
        /// expressed as the number of seconds elapsed since the <see cref="Epoch"/>
        /// value
        /// </summary>
        /// <param name="value">The number of seconds elapsed since the <see cref="Epoch"/></param>
        public UnixTime(double value)
        {
            _value = value;
        }

        /// <inheritdoc />
        public bool Equals(UnixTime other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == GetType() && Equals((UnixTime) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Overloads the <c>==</c> operator to determine the equality of two Unix times based on
        /// their respective values rather than instance equality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the left and right operands represent the same Unix
        /// time; <c>false</c> otherwise</returns>
        public static bool operator ==(UnixTime left, UnixTime right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the <c>!=</c> operator to determine the inequality of two Unix times based on
        /// their respective values rather than instance inequality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the left and right operands represent the different
        /// Unix times; <c>false</c> otherwise</returns>
        public static bool operator !=(UnixTime left, UnixTime right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly casts a .NET <see cref="DateTime"/> value to a <see cref="UnixTime"/>
        /// </summary>
        /// <param name="timestamp">The .NET timestamp value</param>
        public static implicit operator UnixTime(DateTime timestamp)
        {
            return new UnixTime(timestamp);
        }

        /// <summary>
        /// Implicitly casts an integer value representing the number of seconds elapsed
        /// since the <see cref="Epoch"/> to a <see cref="UnixTime"/>
        /// </summary>
        /// <param name="value">The number of seconds elapsed since the Epoch</param>
        public static implicit operator UnixTime(int value)
        {
            return new UnixTime(value);
        }

        /// <summary>
        /// Implicitly casts a floating point value representing the number of seconds elapsed
        /// since the <see cref="Epoch"/> to a <see cref="UnixTime"/>
        /// </summary>
        /// <param name="value">The number of seconds elapsed since the Epoch</param>
        public static implicit operator UnixTime(float value)
        {
            return new UnixTime(value);
        }

        /// <summary>
        /// Implicitly casts a floating point value representing the number of seconds elapsed
        /// since the <see cref="Epoch"/> to a <see cref="UnixTime"/>
        /// </summary>
        /// <param name="value">The number of seconds elapsed since the Epoch</param>
        public static implicit operator UnixTime(double value)
        {
            return new UnixTime(value);
        }
        
        /// <summary>
        /// Implicitly casts a <see cref="UnixTime"/> to a .NET <see cref="DateTime"/>
        /// </summary>
        /// <param name="unixTime">The .NET timestamp value</param>
        public static implicit operator DateTime(UnixTime unixTime)
        {
            return unixTime.ToDateTime();
        }
    }
}
