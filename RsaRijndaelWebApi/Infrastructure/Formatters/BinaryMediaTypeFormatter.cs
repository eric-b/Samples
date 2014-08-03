// Source: https://gist.github.com/aliostad/2519771
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RsaRijndaelWebApi.Infrastructure.Formatters
{
    public class BinaryMediaTypeFormatter : MediaTypeFormatter
    {
        private static readonly Type[] SupportedTypes = new Type[] { typeof(byte[]), typeof(string) };
        private readonly bool _isAsync = false;

        public BinaryMediaTypeFormatter(bool isAsync)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
            _isAsync = isAsync;
        }

        public bool IsAsync
        {
            get
            {
                return _isAsync;
            }
        }

        public override bool CanReadType(Type type)
        {
            return SupportedTypes.Contains(type);
        }

        public override bool CanWriteType(Type type)
        {
            return SupportedTypes.Contains(type);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            Task<object> readTask = GetReadTask(readStream, type);
            if (_isAsync)
            {
                readTask.Start();
            }
            else
            {
                readTask.RunSynchronously();
            }
            return readTask;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            if (value == null)
                value = new byte[0];
            Task writeTask = GetWriteTask(writeStream, value);
            if (_isAsync)
            {
                writeTask.Start();
            }
            else
            {
                writeTask.RunSynchronously();
            }
            return writeTask;
        }

        private Task<object> GetReadTask(Stream stream, Type type)
        {
            return new Task<object>(() =>
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                if (type == typeof(string))
                {
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
                else
                    return ms.ToArray();
            });
        }

        private Task GetWriteTask(Stream stream, object data)
        {
            return new Task(() =>
            {
                if (data != null)
                {
                    MemoryStream ms;
                    if (data is byte[])
                        ms = new MemoryStream((byte[])data);
                    else if (data is string)
                        ms = new MemoryStream(Encoding.UTF8.GetBytes((string)data));
                    else
                        throw new NotSupportedException(string.Format("Type {0} not supported.", data.GetType().Name));
                    ms.CopyTo(stream);
                }
            });
        }
    }
}