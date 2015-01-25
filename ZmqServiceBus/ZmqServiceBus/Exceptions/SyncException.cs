using System;
using System.Linq;

namespace ZmqServiceBus.Exceptions
{
    /// <summary>
    /// Exception thrown when synchronization fails after a socket connection.
    /// </summary>
    [Serializable]
    public sealed class SyncException : Exception
    {
        internal SyncException() : base("Failed to synchronize a socket. Check if the address provided match the corresponding address of the event proxy.")
        {
        }
    }
}