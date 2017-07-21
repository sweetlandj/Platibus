using System;

namespace Platibus.Diagnostics
{
    /// <summary>
    /// Specifies a level of precision for timestamps in InfluxDB
    /// </summary>
    public class InfluxDBPrecision : IEquatable<InfluxDBPrecision>
    {
        /// <summary>
        /// Nanosecond precision
        /// </summary>
        public static readonly InfluxDBPrecision Nanosecond = new InfluxDBPrecision("ns", () => (long)UnixTime.Current.Nanoseconds);

        /// <summary>
        /// Microsecond precision
        /// </summary>
        public static readonly InfluxDBPrecision Microsecond = new InfluxDBPrecision("u", () => (long)UnixTime.Current.Microseconds);

        /// <summary>
        /// Millisecond precision
        /// </summary>
        public static readonly InfluxDBPrecision Millisecond = new InfluxDBPrecision("ms", () => (long)UnixTime.Current.Milliseconds);

        /// <summary>
        /// Second precision
        /// </summary>
        public static readonly InfluxDBPrecision Second = new InfluxDBPrecision("s", () => (long)UnixTime.Current.Seconds);

        /// <summary>
        /// Minute precision
        /// </summary>
        public static readonly InfluxDBPrecision Minute = new InfluxDBPrecision("m", () => (long)UnixTime.Current.Seconds / 60);

        /// <summary>
        /// Hour precision
        /// </summary>
        public static readonly InfluxDBPrecision Hour = new InfluxDBPrecision("h", () => (long)UnixTime.Current.Seconds / 3600);

        private readonly string _name;
        private readonly Func<long> _timestampFunction;

        /// <summary>
        /// Returns the name of the precision value
        /// </summary>
        /// <remarks>
        /// <para>Values are:</para>
        /// <list type="table">
        /// <item>
        /// <term>ns</term>
        /// <definition>Nanosecond (default)</definition>
        /// </item>
        /// <item>
        /// <term>u</term>
        /// <definition>Microsecond</definition>
        /// </item>
        /// <item>
        /// <term>ms</term>
        /// <definition>Millisecond</definition>
        /// </item>
        /// <item>
        /// <term>s</term>
        /// <definition>Second</definition>
        /// </item>
        /// <item>
        /// <term>m</term>
        /// <definition>Minute</definition>
        /// </item>
        /// <item>
        /// <term>h</term>
        /// <definition>Hour</definition>
        /// </item>
        /// </list>
        /// </remarks>
        public string Name { get { return _name; } }

        /// <summary>
        /// Returns the current timetamp at this level of precision
        /// </summary>
        public long GetTimestamp() { return _timestampFunction(); }

        private InfluxDBPrecision(string name, Func<long> timestampFunction)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (timestampFunction == null) throw new ArgumentNullException("timestampFunction");
            _name = name;
            _timestampFunction = timestampFunction;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _name;
        }

        /// <inheritdoc />
        public bool Equals(InfluxDBPrecision other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(_name, other._name);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((InfluxDBPrecision) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        /// <summary>
        /// Overloads the <c>==</c> operator to evaluate equalty based on value rather than on
        /// instance equality.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the <paramref name="left"/> value is equivalent to
        /// the <paramref name="right"/> value; <c>false</c> otherwise</returns>
        public static bool operator ==(InfluxDBPrecision left, InfluxDBPrecision right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the <c>!=</c> operator to evaluate inequalty based on value rather than on
        /// instance inequality.
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the <paramref name="left"/> value is not equivalent to
        /// the <paramref name="right"/> value; <c>false</c> otherwise</returns>
        public static bool operator !=(InfluxDBPrecision left, InfluxDBPrecision right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns the <see cref="InfluxDBPrecision"/> instance corresponding to the specified
        /// <paramref name="name"/>
        /// </summary>
        /// <param name="name">The precision name (e.g. "ns", "u", "ms", "s", "m", "h")</param>
        /// <returns>Returns the <see cref="InfluxDBPrecision"/> instance corresponding to the specified
        /// <paramref name="name"/></returns>
        public static InfluxDBPrecision Parse(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            switch (name.Trim().ToLower())
            {
                case "ns": return Nanosecond;
                case "u": return Microsecond;
                case "ms": return Millisecond;
                case "s": return Second;
                case "m": return Minute;
                case "h": return Hour;
                default:
                    throw new ArgumentOutOfRangeException("name", name, "Unsupported precision");
            }
        }
    }
}
