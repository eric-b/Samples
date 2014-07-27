using System;
using System.IO;
using System.Linq;

namespace RsaRijndaelWebApi.Infrastructure.Cryptography
{
    public interface IRijndaelEncryptor : IDisposable
    {
        byte[] Encrypt(Stream data);

        byte[] Encrypt(byte[] data);
    }
}