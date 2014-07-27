using System;
using System.Linq;
using System.Security.Cryptography;

namespace RsaRijndaelWebApi.Infrastructure.Cryptography
{
    public class Rsa : IDisposable
    {
        private readonly RSACryptoServiceProvider _rsaPrivate;

        public Rsa(string xmlKey)
        {
            if (xmlKey == null)
                throw new ArgumentNullException("xmlKey");
            
            _rsaPrivate = new RSACryptoServiceProvider();
            _rsaPrivate.FromXmlString(xmlKey);
        }

        public string GetPublicKey()
        {
            return _rsaPrivate.ToXmlString(false);
        }

        public byte[] Decrypt(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("data");
            return _rsaPrivate.Decrypt(key, true);
        }

        public byte[] Encrypt(byte[] data)
        {
            if (_rsaPrivate == null)
                throw new InvalidOperationException("Aucune clé n'a été spécifiée.");
            return _rsaPrivate.Encrypt(data, true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rsaPrivate.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}