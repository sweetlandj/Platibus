// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using System.Diagnostics;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Represents a Unix time value
    /// </summary>
    [DebuggerDisplay("{" + nameof(Seconds) + ",nq}")]
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

        /// <summary>
        /// Returns the seconds elapsed since the <see cref="Epoch"/>
        /// </summary>
        public double Seconds { get; }

        /// <summary>
        /// Returns the milliseconds elapsed since the <see cref="Epoch"/>
        /// </summary>
        public double Milliseconds => Seconds * 1e+3;

        /// <summary>
        /// Returns the microseconds elapsed since the <see cref="Epoch"/>
        /// </summary>
        public double Microseconds => Seconds * 1e+6;

        /// <summary>
        /// Returns the nanoseconds elapsed since the <see cref="Epoch"/>
        /// </summary>
        public double Nanoseconds => Seconds * 1e+9;

        /// <summary>
        /// Initializes a new <see cref="UnixTime"/> from the specified .NET <see cref="DateTime"/>
        /// value
        /// </summary>
        /// <param name="timestamp">The .NET date/time value</param>
        public UnixTime(DateTime timestamp)
        {
            var utcTimestamp = timestamp.ToUniversalTime();

            // Round to nanosecond precision
            Seconds = (utcTimestamp - Epoch).TotalSeconds;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Seconds.ToString("F");
        }

        /// <summary>
        /// Returns the .NET <see cref="DateTime"/> representation of the Unix time value
        /// </summary>
        /// <returns>Returns the .NET <see cref="DateTime"/> representation of the Unix time value</returns>
        public DateTime ToDateTime()
        {
            return Epoch + TimeSpan.FromSeconds(Seconds);
        }
        
        /// <summary>
        /// Initializes a new <see cref="UnixTime"/> from the specified <paramref name="seconds"/>
        /// expressed as the number of seconds elapsed since the <see cref="Epoch"/>
        /// value
        /// </summary>
        /// <param name="seconds">The number of seconds elapsed since the <see cref="Epoch"/></param>
        public UnixTime(double seconds)
        {
            Seconds = seconds;
        }

        /// <inheritdoc />
        public bool Equals(UnixTime other)
        {
            return Seconds.Equals(other.Seconds);
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
            return Seconds.GetHashCode();
        }
        
        /// <summary>
        /// Returns the <see cref="UnixTime"/> corresponding to the specified number of 
        /// <paramref name="seconds"/> since the <see cref="Epoch"/>
        /// </summary>
        /// <param name="seconds">The number of seconds elapsed since the Epoch</param>
        public static UnixTime FromSeconds(double seconds)
        {
            return new UnixTime(seconds);
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
        /// Implicitly casts a <see cref="UnixTime"/> to a .NET <see cref="DateTime"/>
        /// </summary>
        /// <param name="unixTime">The .NET timestamp value</param>
        public static implicit operator DateTime(UnixTime unixTime)
        {
            return unixTime.ToDateTime();
        }
    }
}
