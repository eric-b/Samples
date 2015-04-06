using System;
using System.Linq;
using NUnit.Framework;
using ZmqReqRep.Client;
using ZmqReqRep.Server;

namespace ZmqReqRep.Tests
{
    [TestFixture]
    public class SimpleReqRepTest
    {
        [Test]
        [Repeat(2)]
        public void Test()
        {
            string address = string.Format("tcp://localhost:{0}", Helper.GetAvailablePort());
            using (new RepServer(address, 1))
            using (var context = NetMQ.NetMQContext.Create())
            using (var client = new ReqSocket(address, context))
            {
                Console.WriteLine("Client connected.");
                for (int i = 0; i < 4; i++)
                {
                    string response = client.SendRequest(string.Format("Request #{0}", i+1));
                    Console.WriteLine(response);
                    Assert.AreEqual(string.Format("Reply (Request #{0})", i+1), response);
                }
            }
        }
    }
}