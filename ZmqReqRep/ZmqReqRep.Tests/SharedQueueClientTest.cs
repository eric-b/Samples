using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZmqReqRep.Client;
using ZmqReqRep.Server;

namespace ZmqReqRep.Tests
{
    [TestFixture]
    public sealed class SharedQueueClientTest
    {
        [Test]
        [Repeat(1)]
        public void Test()
        {
            string address = string.Format("tcp://localhost:{0}", Helper.GetAvailablePort());
            using (new RepServer(address, 1))
                using (var clientFactory = new ClientFactory(address))
                {
                    List<Task> tasks = new List<Task>();
                    for (int i = 0; i < 4; i++)
                        tasks.Add(Task.Factory.StartNew(RequestThread, clientFactory));

                    Task.WaitAll(tasks.ToArray());
                }
        }

        private void RequestThread(object clientFactory)
        {
            IReqSocket client = null;
            try
            {
                client = ((ClientFactory)clientFactory).Create();

                Console.WriteLine("Client connected.");
                string response = client.SendRequest(string.Format("Request #{0}", Thread.CurrentThread.ManagedThreadId));
                Console.WriteLine(response);
                Assert.AreEqual(string.Format("Reply (Request #{0})", Thread.CurrentThread.ManagedThreadId), response);
            }
            finally
            {
                if (client != null)
                    ((ClientFactory)clientFactory).Release(client);
            }
        }
    }
}