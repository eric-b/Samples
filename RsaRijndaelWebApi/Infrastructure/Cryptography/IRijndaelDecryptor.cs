using System;
using System.Linq;

namespace RsaRijndaelWebApi.Infrastructure.Cryptography
{
    public interface IRijndaelDecryptor : IDisposable
    {
        string Decrypt(string data);

        byte[] Decrypt(byte[] data);
    }
}