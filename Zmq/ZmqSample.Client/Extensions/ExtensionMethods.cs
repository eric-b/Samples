using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NetMQ;
using ZmqSample.Client.Model;

namespace ZmqSample.Client.Extensions
{
    internal static class ExtensionMethods
    {
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        public static byte[] SerializeMessage(this IMessage msg)
        {
            if (msg == null)
                throw new ArgumentNullException("o");
            using (var ms = new MemoryStream())
            {
                BinaryFormatter.Serialize(ms, msg);
                return ms.ToArray();
            }
        }

        public static IMessage DeserializeMessage(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            using (var ms = new MemoryStream(data, false))
            {
                return (IMessage)BinaryFormatter.Deserialize(ms);
            }
        }

        public static bool CaptureMqExceptions(this Action action)
        {
            return CaptureMqExceptions<NetMQException>(action);
        }

        public static bool CaptureMqExceptions<T>(this Action action) where T : NetMQException
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
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public static TResult CaptureMqExceptions<TResult>(this Func<TResult> func)
        {
            return CaptureMqExceptions<NetMQException, TResult>(func);
        }

        public static TResult CaptureMqExceptions<T, TResult>(this Func<TResult> func) where T : NetMQException
        {
            if (func == null)
                throw new ArgumentNullException("action");
            try
            {
                return func();
            }
            catch (T ex)
            {
                Debug.WriteLine(ex.ToString());
                return default(TResult);
            }
        }
    }
}