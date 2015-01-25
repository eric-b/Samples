using System;
using System.Linq;

namespace ZmqServiceBus.Exceptions
{
    /// <summary>
    /// Exception thrown when a deserialization error occurs.
    /// </summary>
    [Serializable]
    public sealed class DeserializationException : SerializationException
    {
        internal DeserializationException(Exception innerEx, string message, params object[] args) : base(innerEx, message, args)
        {
        }
    }
}