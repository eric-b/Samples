using System;
using System.Linq;
using ProtoBuf;

namespace ZmqServiceBus.Internals
{
    /// <summary>
    /// Internal event used to synchronize 
    /// the connection of a socket.
    /// </summary>
    [ProtoContract]
    internal sealed class SyncEvent
    {
        public static readonly byte[] EventPrefix = BitConverter.GetBytes(EventTopic);

        public const long EventTopic = -1;

        [ProtoMember(1)]
        public int SubscriberInstanceId { get; set; }

        public SyncEvent(int subscriberInstanceId)
        {
            SubscriberInstanceId = subscriberInstanceId;
        }

        /// <summary>
        /// Private constructor for deserialization.
        /// </summary>
        private SyncEvent()
        {
        }
    }
}