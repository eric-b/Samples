using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RsaRijndaelWebApi.Infrastructure.Cryptography
{
    public class Rijndael : IRijndaelDecryptor, IRijndaelEncryptor, IDisposable
    {
        private readonly ICryptoTransform _cryptoTransform;

        public Rijndael(ICryptoTransform actor)
        {
            _cryptoTransform = actor;
        }

        public string Decrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;
            using (var input = new MemoryStream(Convert.FromBase64String(data), false))
            {
                using (var cryptoStream = new CryptoStream(input, _cryptoTransform, CryptoStreamMode.Read))
                {
                    using (var output = new StreamReader(cryptoStream, Encoding.UTF8))
                    {
                        return output.ReadToEnd();
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            if (data == null)
                return data;
            using (var input = new MemoryStream(data, false))
            {
                using (var cryptoStream = new CryptoStream(input, _cryptoTransform, CryptoStreamMode.Read))
                {
                    var output = new MemoryStream();
                    cryptoStream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        public byte[] Encrypt(Stream data)
        {
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, _cryptoTransform, CryptoStreamMode.Write))
                {
                    data.CopyTo(cryptoStream);
                }
                return ms.ToArray();
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, _cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cryptoTransform.Dispose();
            }
        }
    }
}