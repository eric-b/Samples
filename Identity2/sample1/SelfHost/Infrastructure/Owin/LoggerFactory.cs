using System;
using System.Linq;
using Microsoft.Owin.Logging;

namespace Identity2.Sample1.SelfHost.Infrastructure.Owin
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            return new Logger();
        }

        private class Logger : ILogger
        {
            public bool WriteCore(System.Diagnostics.TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                Console.WriteLine(string.Format("{0}: {1}", eventType, formatter(state, exception)));
                return true;
            }
        }
    }
}