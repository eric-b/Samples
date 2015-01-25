using System;
using System.Diagnostics;
using System.Linq;
using NetMQ;

namespace ZmqServiceBus
{
    internal static class ExtensionMethods
    {
        public static bool CaptureMqExceptions<T>(this ITraceWriter traceWriter, Action action) where T : NetMQException
        {
            if (action == null)
                throw new ArgumentNullException("action");
            try
            {
                action();
                return true;
            }
            catch (T ex)
            {
                if (traceWriter != null)
                    traceWriter.Error(ex);
                else
                    Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public static bool CaptureMqExceptions(this ITraceWriter traceWriter, Action action)
        {
            return CaptureMqExceptions<NetMQException>(traceWriter, action);
        }

        public static TResult CaptureMqExceptions<T, TResult>(this ITraceWriter traceWriter, Func<TResult> func) where T : NetMQException
        {
            if (func == null)
                throw new ArgumentNullException("action");
            try
            {
                return func();
            }
            catch (T ex)
            {
                if (traceWriter != null)
                    traceWriter.Error(ex);
                else
                    Debug.WriteLine(ex.ToString());
                return default(TResult);
            }
        }
    }
}