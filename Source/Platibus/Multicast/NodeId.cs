using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Platibus.Multicast
{
    internal class NodeId : IEquatable<NodeId>
    {
        private const int HostBytesLength = 4;
        private const int RandomBytesLength = 4;
        public const int Length = HostBytesLength + RandomBytesLength;

        private static readonly RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();
        private static readonly byte[] HostBytes = new byte[HostBytesLength];

        static NodeId()
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var machineNameHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName));
            Array.Copy(machineNameHash, 0, HostBytes, 0, HostBytesLength);
        }

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

        public NodeId(byte[] buffer) : this (buffer, 0)
        {
        }

        public NodeId(byte[] buffer, int start)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length < start + Length) throw new ArgumentOutOfRangeException("buffer");
            Array.Copy(buffer, start, _bytes, 0, Length);
        }

        public override string ToString()
        {
            var numericValue = BitConverter.ToInt64(_bytes, 0);
            return numericValue.ToString("X");
        }

        public bool Equals(NodeId other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) 
                || StructuralComparisons.StructuralEqualityComparer.Equals(_bytes, other._bytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NodeId) obj);
        }

        public override int GetHashCode()
        {
            return _bytes.GetHashCode();
        }

        public static bool operator ==(NodeId left, NodeId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NodeId left, NodeId right)
        {
            return !Equals(left, right);
        }

        public static implicit operator NodeId(byte[] value)
        {
            return value == null ? null : new NodeId(value);
        }

        public static implicit operator byte[] (NodeId nodeId)
        {
            return nodeId == null ? null : nodeId._bytes;
        }
    }
}
