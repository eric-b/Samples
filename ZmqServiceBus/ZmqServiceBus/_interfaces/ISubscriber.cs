using System;
using System.Linq;

namespace ZmqServiceBus
{
    /// <summary>
    /// Service bus events receiver.
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// Event received.
        /// </summary>
        event EventHandler<MessageEventArgs> OnMessage;
    }
}