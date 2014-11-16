using System;
using System.Linq;

namespace ZmqSample.Client.Model
{
    [Serializable]
    public sealed class PushMsg : IMessage
    {
        public PushMsg(string name)
        {
            Name = name;
            OnUtc = DateTime.UtcNow;
        }

        public string Name { get; set; }

        public DateTime OnUtc { get; set; }

        public override string ToString()
        {
            return string.Format("{0} (sent on {1:HH:mm:ss.fff})", Name, OnUtc);
        }
    }
}