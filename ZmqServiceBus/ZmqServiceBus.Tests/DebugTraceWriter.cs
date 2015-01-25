using System;
using System.Linq;

namespace ZmqServiceBus.Tests
{
    internal sealed class DebugTraceWriter : ITraceWriter
    {
        public void Debug(string messageFormat, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(messageFormat, args);
        }

        public void Warn(string messageFormat, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(messageFormat, args);
        }

        public void Error(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        public void Error(Exception ex, string messageFormat, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0}{1}{2}", string.Format(messageFormat, args), Environment.NewLine, ex.ToString()));
        }
    }
}