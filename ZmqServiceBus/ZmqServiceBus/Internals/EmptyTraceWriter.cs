using System;
using System.Linq;

namespace ZmqServiceBus.Internals
{
    internal sealed class EmptyTraceWriter : ITraceWriter
    {
        private static EmptyTraceWriter _instance = new EmptyTraceWriter();

        public static EmptyTraceWriter Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public void Debug(string messageFormat, params object[] args)
        {
        }

        public void Warn(string messageFormat, params object[] args)
        {
        }

        public void Error(Exception ex)
        {
        }

        public void Error(Exception ex, string messageFormat, params object[] args)
        {
        }
    }
}