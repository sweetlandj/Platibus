using System;
using System.Text;

namespace Platibus.Multicast
{
    internal class SubscriptionTrackingDatagram
    {
        /// <summary>
        /// First byte of the datagram that identifies this as a subscription datagram with the
        /// expected format
        /// </summary>
        private const byte TypeId = 243;
        
        private const byte NullTerminator = 0;
        private const int NodeIdLenth = NodeId.Length;
        private const int TypeIdLength = sizeof(byte);
        private const int ActionTypeLength = sizeof(byte);
        private const int TTLLength = sizeof(int);
        private const int NullTerminatorLength = sizeof(byte);

        private const int MinTopicLength = 1;
        private const int MinUriLength = 7;

        private const int MinLength = NodeIdLenth + TypeIdLength + ActionTypeLength
                                      + TTLLength + MinTopicLength + NullTerminatorLength
                                      + MinUriLength + NullTerminatorLength;

        private static readonly Encoding Encoding = Encoding.UTF8;

        private readonly NodeId _nodeId;
        private readonly ActionType _action;
        private readonly TopicName _topic;
        private readonly Uri _subscriberUri;
        private readonly TimeSpan _ttl;

        public NodeId NodeId { get { return _nodeId; } }
        public ActionType Action { get { return _action; } }
        public TopicName Topic { get { return _topic; } }
        public Uri SubscriberUri { get { return _subscriberUri; } }
        public TimeSpan TTL { get { return _ttl; } }

        public SubscriptionTrackingDatagram(NodeId nodeId, ActionType action, TopicName topic, Uri subscriberUri, TimeSpan ttl = default(TimeSpan))
        {
            if (topic == null) throw new ArgumentNullException("topic");
            if (subscriberUri == null) throw new ArgumentNullException("subscriberUri");
            _nodeId = nodeId;
            _action = action;
            _topic = topic;
            _subscriberUri = subscriberUri;
            _ttl = ttl;
        }

        public static SubscriptionTrackingDatagram Decode(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length < MinLength) throw new ArgumentOutOfRangeException("buffer", "Buffer must be at least " + MinLength + " bytes");

            var off = 0;
            var nodeId = new NodeId(buffer, off);
            off += NodeIdLenth;

            // Type ID already verified
            var typeId = buffer[off];
            if (typeId != TypeId) throw new ArgumentOutOfRangeException("buffer", typeId, "Incorrect type ID");
            off += TypeIdLength;

            var action = (ActionType) buffer[off];
            off += ActionTypeLength;

            var ttl = TimeSpan.FromSeconds(ByteUtils.ReadInt32(buffer, off));
            off += TTLLength;

            var topicEnd = Array.IndexOf(buffer, NullTerminator, off);
            if (topicEnd < 0) throw new ArgumentOutOfRangeException("buffer", "Missing null terminator for topic");
            var topicLength = topicEnd - off;
            var topic = Encoding.GetString(buffer, off, topicLength);
            off += topicLength + NullTerminatorLength;

            var subscriberUriEnd = Array.IndexOf(buffer, NullTerminator, off);
            if (subscriberUriEnd < 0) throw new ArgumentOutOfRangeException("buffer", "Missing null terminator for subscriber URI");
            var subscriberUriLength = subscriberUriEnd - off;
            var subscriberUri = new Uri(Encoding.GetString(buffer, off, subscriberUriLength));

            return new SubscriptionTrackingDatagram(nodeId, action, topic, subscriberUri, ttl);
        }

        public byte[] Encode()
        {
            var topicBytes = Encoding.GetBytes(_topic);
            var subscriberUriBytes = Encoding.GetBytes(_subscriberUri.ToString());
            
            var topicLen = topicBytes.Length;
            var subscriberUriLen = subscriberUriBytes.Length;
            var datagramLen = NodeIdLenth + TypeIdLength + ActionTypeLength
                              + TTLLength + topicLen + NullTerminatorLength
                              + subscriberUriLen + NullTerminatorLength;

            var buffer = new byte[datagramLen];

            var off = 0;
            Array.Copy(_nodeId, 0, buffer, off, NodeId.Length);
            off += NodeIdLenth;

            buffer[off] = TypeId;
            off += TypeIdLength;

            buffer[off] = (byte)_action;
            off += ActionTypeLength;

            ByteUtils.WriteInt32((int)_ttl.TotalSeconds, buffer, off);
            off += TTLLength;

            Array.Copy(topicBytes, 0, buffer, off, topicLen);
            off += topicLen;

            buffer[off] = NullTerminator;
            off++;

            Array.Copy(subscriberUriBytes, 0, buffer, off, subscriberUriLen);
            off += subscriberUriLen;

            buffer[off] = NullTerminator;
            return buffer;
        }

        public static implicit operator byte[](SubscriptionTrackingDatagram datagram)
        {
            return datagram == null ? null : datagram.Encode();
        }

        public static implicit operator SubscriptionTrackingDatagram(byte[] buffer)
        {
            return buffer == null ? null : Decode(buffer);
        }

        public enum ActionType : byte
        {
            Add = 0,
            Remove = 1
        }
    }
}
