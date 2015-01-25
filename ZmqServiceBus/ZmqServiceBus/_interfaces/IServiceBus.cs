using System;
using System.Linq;

namespace ZmqServiceBus
{
    /// <summary>
    /// Service bus: it is basically
    /// a factory to create Publishers (send events) 
    /// and Subscribers (receive events).
    /// </summary>
    public interface IServiceBus : IDisposable
    {
        /// <summary>
        /// <para>Creates a subscriber listening for
        /// the specified events.</para>
        /// <para>The instance returned must be
        /// released after use with the Release
        /// method.</para>
        /// </summary>
        /// <param name="subscribeToEventCodes">Identificators
        /// of the event types to receive. Can be empty
        /// to receive all event types.</param>
        /// <returns>A subscriber</returns>
        ISubscriber CreateSubscriber(params long[] subscribeToEventCodes);

        /// <summary>
        /// <para>Creates a publisher.</para>
        /// <para>The instance returned must be
        /// released after use with the Release
        /// method.</para>
        /// </summary>
        /// <returns>A publisher</returns>
        IPublisher CreatePublisher();

        /// <summary>
        /// Release the subscriber.
        /// </summary>
        /// <param name="instance"></param>
        void Release(ISubscriber instance);

        /// <summary>
        /// Release the publisher.
        /// </summary>
        /// <param name="instance"></param>
        void Release(IPublisher instance);
    }
}