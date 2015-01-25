using System;
using System.Linq;

namespace ZmqServiceBus
{
    /// <summary>
    /// Service bus events transmitter.
    /// </summary>
    public interface IPublisher
    {
        void Publish<T>(long eventCode, T message) where T : class;
    }
}