using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Platibus.Multicast
{
    /// <summary>
    /// An identifier that uniquel identifies a node participating in UDP multicast communications
    /// </summary>
    public class NodeId : IEquatable<NodeId>
    {
        private const int HostBytesLength = 4;
        private const int RandomBytesLength = 4;

        /// <summary>
        /// The length of a marshaled node ID in bytes
        /// </summary>
        public const int Length = HostBytesLength + RandomBytesLength;

        private static readonly RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();
        private static readonly byte[] HostBytes = new byte[HostBytesLength];

        static NodeId()
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var machineNameHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
            Array.Copy(machineNameHash, 0, HostBytes, 0, HostBytesLength);
        }

        /// <summary>
        /// Generates a new node ID based on the host name and random number generator
        /// </summary>
        /// <returns>Returns a new node ID</returns>
        public static NodeId Generate()
        {
            var randomBytes = new byte[RandomBytesLength];
            RNG.GetBytes(randomBytes);
            var nodeIdBytes = new byte[Length];
            Array.Copy(HostBytes, 0, nodeIdBytes, 0, HostBytesLength);
            Array.Copy(randomBytes, 0, nodeIdBytes, HostBytesLength, RandomBytesLength);
            return new NodeId(nodeIdBytes);
        }
        
        private readonly byte[] _bytes = new byte[Length];

        /// <summary>
        /// Initializes a <see cref="NodeId"/> from the specified byte array
        /// </summary>
        /// <param name="buffer">The byte array whose first <see cref="Length"/> bytes are the
        /// node ID</param>
        public NodeId(byte[] buffer) : this (buffer, 0)
        {
        }

        /// <summary>
        /// Initializes a <see cref="NodeId"/> from the specified byte array
        /// </summary>
        /// <param name="start">The first position of the node ID in the specified 
        /// <paramref name="buffer"/></param>
        /// <param name="buffer">The byte array containing the node ID beginning at the specified
        /// <paramref name="start"/> index</param>
        public NodeId(byte[] buffer, int start)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (buffer.Length < start + Length) throw new ArgumentOutOfRangeException(nameof(buffer));
            Array.Copy(buffer, start, _bytes, 0, Length);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var numericValue = BitConverter.ToInt64(_bytes, 0);
            return numericValue.ToString("X");
        }

        /// <inheritdoc />
        public bool Equals(NodeId other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) 
                || StructuralComparisons.StructuralEqualityComparer.Equals(_bytes, other._bytes);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NodeId) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _bytes.GetHashCode();
        }

        /// <summary>
        /// Overloads the equality operator to determine the equality of two <see cref="NodeId"/>s
        /// based on their respective values vice instance equality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two node IDs represent the same value; <c>false</c>
        /// otherwise</returns>
        public static bool operator ==(NodeId left, NodeId right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloads the inequality operator to determine the inequality of two 
        /// <see cref="NodeId"/>s based on their respective values vice instance equality
        /// </summary>
        /// <param name="left">The left operand</param>
        /// <param name="right">The right operand</param>
        /// <returns>Returns <c>true</c> if the two node IDs do not represent the same value; 
        /// <c>false</c> otherwise</returns>
        public static bool operator !=(NodeId left, NodeId right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns the node ID as an array of bytes
        /// </summary>
        /// <returns>Returns the node ID as an array of bytes</returns>
        public byte[] GetBytes()
        {
            // Return a copy to preserve immutability
            var returnValue = new byte[_bytes.Length];
            Array.Copy(_bytes, returnValue, _bytes.Length);
            return returnValue;
        }
    }
}
