using System;
using System.Linq;
using ProtoBuf;

namespace ZmqServiceBus.Tests
{
    [ProtoContract]
    internal sealed class TestEvent
    {
        [ProtoMember(1)]
        public long SendTimeTicks { get; set; }


    }
}