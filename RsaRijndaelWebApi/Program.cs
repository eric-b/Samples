using System;
using System.Configuration;
using System.Text;
using Microsoft.Owin.Hosting;
using RsaRijndaelWebApi.Infrastructure.Cryptography;

namespace RsaRijndaelWebApi
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var baseAddress = ConfigurationManager.AppSettings["baseAddress"];
            using (WebApp.Start<Infrastructure.AppStartup.OwinAppStartup>(baseAddress))
            {
                // Sample usage :
                var client = new System.Net.Http.HttpClient();

                var cipher = (Cipher)Infrastructure.AppStartup.IocInitializer.SetUp().GetService(typeof(Cipher));
                var publicKey = cipher.GetPublicKey();
                Console.WriteLine("Public Key:\r\n{0}\r\n", publicKey);

                var publicRsa = new Rsa(publicKey);
                var rijndaelKey = cipher.GenerateKey();
                client.DefaultRequestHeaders.Add(Infrastructure.ActionFilters.InternalActionFilterAttribute.HeaderKey, Convert.ToBase64String(publicRsa.Encrypt(rijndaelKey.key)));
                client.DefaultRequestHeaders.Add(Infrastructure.ActionFilters.InternalActionFilterAttribute.HeaderIV, Convert.ToBase64String(publicRsa.Encrypt(rijndaelKey.IV)));
                
                using (var encryptor = cipher.CreateDataEncryptor(rijndaelKey.key, rijndaelKey.IV))
                {
                    const string sampleName = "Smith";
                    var requestUri = string.Format("{0}api/SayHello/{1}", baseAddress, Convert.ToBase64String(encryptor.Encrypt(Encoding.UTF8.GetBytes(sampleName))));
                    Console.WriteLine("GET {0} ...", requestUri);
                    var response = client.GetAsync(requestUri).Result;

                    var content = response.Content.ReadAsByteArrayAsync().Result;

                    using (var decryptor = cipher.CreateDataDecryptor(rijndaelKey.key, rijndaelKey.IV))
                    {
                        var decipheredContent = Encoding.UTF8.GetString(decryptor.Decrypt(content));
                        Console.WriteLine("{0}\r\n{1}", response, decipheredContent);

                        System.Diagnostics.Debug.Assert(
                            decipheredContent == string.Format("\"Hello {0}\"", sampleName), 
                            string.Format("Unexpected result: {0}", decipheredContent));
                    }
                }
                Console.ReadKey(true);
            }
        }
    }
}