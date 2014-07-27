using System;
using System.Linq;
using System.Security.Cryptography;

namespace RsaRijndaelWebApi.Infrastructure.Cryptography
{
    public class Cipher : IDisposable
    {
        private readonly RijndaelManaged _rijndaelManaged;
        private readonly RsaRijndaelWebApi.Infrastructure.Cryptography.Rsa _rsa;

        public Cipher(string xmlPrivateKey)
        {
            _rsa = new RsaRijndaelWebApi.Infrastructure.Cryptography.Rsa(xmlPrivateKey);
            _rijndaelManaged = new RijndaelManaged();
        }

        public string GetPublicKey()
        {
            return _rsa.GetPublicKey();
        }

        public RsaRijndaelWebApi.Infrastructure.Cryptography.RijndaelKey GenerateKey()
        {
            using (var r = new RijndaelManaged())
            {
                r.GenerateKey();
                r.GenerateIV();
                return new RsaRijndaelWebApi.Infrastructure.Cryptography.RijndaelKey()
                {
                    key = r.Key,
                    IV = r.IV
                };
            }
        }

        public RsaRijndaelWebApi.Infrastructure.Cryptography.IRijndaelEncryptor CreateDataEncryptor(byte[] key, byte[] vector)
        {
            if (_rijndaelManaged == null)
                throw new InvalidOperationException("Opération non supportée dans l'état courant de cet objet.");
            return new RsaRijndaelWebApi.Infrastructure.Cryptography.Rijndael(_rijndaelManaged.CreateEncryptor(key, vector));
        }

        public RsaRijndaelWebApi.Infrastructure.Cryptography.IRijndaelDecryptor CreateDataDecryptor(byte[] key, byte[] vector)
        {
            if (_rijndaelManaged == null)
                throw new InvalidOperationException("Opération non supportée dans l'état courant de cet objet.");
            return new RsaRijndaelWebApi.Infrastructure.Cryptography.Rijndael(_rijndaelManaged.CreateDecryptor(key, vector));
        }

        public byte[] DecryptKey(byte[] key)
        {
            return _rsa.Decrypt(key);
        }

        public byte[] EncryptKey(byte[] data)
        {
            return _rsa.Encrypt(data);
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
                _rijndaelManaged.Dispose();
                _rsa.Dispose();
            }
        }
    }
}