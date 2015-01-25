using System;
using System.IO;
using System.Linq;
using ProtoBuf;

using ZmqServiceBus.Exceptions;

namespace ZmqServiceBus
{
    /// <summary>
    /// Represents an event received by an ISubscriber.
    /// </summary>
    public sealed class MessageEventArgs
    {
        private readonly long _eventCode;
        private readonly byte[] _data;
        private object _message;

        /// <summary>
        /// Gets an identificator of the event type.
        /// </summary>
        public long EventCode
        {
            get
            {
                return this._eventCode;
            }
        }

        internal MessageEventArgs(long eventCode, byte[] data)
        {
            _eventCode = eventCode;
            _data = data;
        }

        /// <summary>
        /// Deserializes the event.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <returns></returns>
        /// <exception cref="DeserializationException"></exception>
        public T GetMessage<T>() where T : new()
        {
            if (_message != null)
                return (T)_message;

            var ms = new MemoryStream(_data, 0, _data.Length, false);
            try
            {
                _message = Serializer.Deserialize<T>(ms);
            }
            catch (InvalidOperationException ex)
            {
                throw new DeserializationException(ex, "Error when deserializing event of type {0}. Check if that type is a valid Protobuf contract (annotated with ProtoContractAttribute).", typeof(T).FullName);
            }
            catch (ProtoException ex)
            {
                throw new DeserializationException(ex, "Error when deserializing event of type {0}. Check if that type matches the event topic received.", typeof(T).FullName);
            }
            catch (Exception ex)
            {
                throw new DeserializationException(ex, "Error when deserializing event of type {0}.", typeof(T).FullName);
            }
            finally
            {
                ms.Dispose();
            }
            return (T)_message;
        }
    }
}