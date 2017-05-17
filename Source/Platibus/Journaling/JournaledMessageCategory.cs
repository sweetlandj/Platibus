using System;

namespace Platibus.Journaling
{
    /// <summary>
    /// Categories of journaled messages
    /// </summary>
    public sealed class JournaledMessageCategory : IEquatable<JournaledMessageCategory>
    {
        /// <summary>
        /// Messages that were sent by this instance
        /// </summary>
        public static readonly JournaledMessageCategory Sent = new JournaledMessageCategory("Sent");

        /// <summary>
        /// Messages that were received by this instance
        /// </summary>
        /// <remarks>
        /// Includes direct messages to this instance; replies; and publications to which this
        /// instance is subscribed.
        /// </remarks>
        public static readonly JournaledMessageCategory Received = new JournaledMessageCategory("Received");

        /// <summary>
        /// Messages that were published by this instance
        /// </summary>
        public static readonly JournaledMessageCategory Published = new JournaledMessageCategory("Published");

        private readonly string _value;

        private JournaledMessageCategory(string value)
        {
            _value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _value;
        }

        /// <inheritdoc />
        public bool Equals(JournaledMessageCategory other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) ||
                   string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return ReferenceEquals(this, obj) || Equals(obj as JournaledMessageCategory);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value != null ? _value.ToLower().GetHashCode() : 0;
        }

        /// <summary>
        /// Overloads the <c>==</c> operator to determine equality based on value rather than
        /// instance equality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if both values are equal; <c>false</c> otherwise</returns>
        public static bool operator ==(JournaledMessageCategory left, JournaledMessageCategory right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the <c>==</c> operator to determine inequality based on value rather than
        /// instance inequality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>false</c> if both values are equal; <c>true</c> otherwise</returns>
        public static bool operator !=(JournaledMessageCategory left, JournaledMessageCategory right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="JournaledMessageCategory"/> into its string 
        /// representation
        /// </summary>
        /// <param name="category">The message journal category</param>
        /// <returns>Returns <c>null</c> if <paramref name="category"/> is <c>null</c>.  Otherwise
        /// returns the string representation of the specified category</returns>
        public static implicit operator string(JournaledMessageCategory category)
        {
            return category == null ? null : category.ToString();
        }

        /// <summary>
        /// Implicitly converts a string into a <see cref="JournaledMessageCategory"/>
        /// representation
        /// </summary>
        /// <param name="value">The string representation of the message journal category</param>
        /// <returns>Returns <c>null</c> if <paramref name="value"/> is <c>null</c> or whitespace.  
        /// Otherwise returns a <see cref="JournaledMessageCategory"/> instance wrapping the 
        /// specified string value</returns>
        public static implicit operator JournaledMessageCategory(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : new JournaledMessageCategory(value.Trim());
        }
    }
}
