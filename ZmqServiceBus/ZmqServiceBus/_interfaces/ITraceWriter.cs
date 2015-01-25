using System;
using System.Linq;

namespace ZmqServiceBus
{
    /// <summary>
    /// Represents a trace writer.
    /// </summary>
    public interface ITraceWriter
    {
        void Debug(string messageFormat, params object[] args);

        void Warn(string messageFormat, params object[] args);

        void Error(Exception ex);

        void Error(Exception ex, string messageFormat, params object[] args);
    }
}