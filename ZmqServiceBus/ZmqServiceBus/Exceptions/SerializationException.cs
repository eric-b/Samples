using System;
using System.Linq;

namespace ZmqServiceBus.Exceptions
{
    /// <summary>
    /// Exception thrown when a serialization error occurs.
    /// </summary>
    [Serializable]
    public class SerializationException : Exception
    {
        internal SerializationException(Exception innerEx, string message, params object[] args) : base(string.Format(message, args), innerEx)
        {
        }
    }
}