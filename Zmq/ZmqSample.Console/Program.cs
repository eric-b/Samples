using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ZmqSample.Client;
using ZmqSample.Client.Model;

namespace ZmqSample
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var freePort = GetFreePort();
                var bindAddress = string.Format("tcp://*:{0}", freePort);
                var connectAddress = string.Format("tcp://localhost:{0}", freePort);
                Console.WriteLine(string.Format("Port used: {0}", freePort));

                DemoPushPull(connectAddress, bindAddress);
                DemoPublishSubscribe(connectAddress, bindAddress);
                DemoRequestReply(connectAddress, bindAddress);
                DemoRequestReplyWithSingleClientContext(connectAddress, bindAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey(true);
        }

        private static void DemoPushPull(string connectAddress, string bindAddress)
        {
            Console.WriteLine("\r\nPattern Push/Pull :");
            using (var client = new Client.PushClient(connectAddress))
            using (var server = new Server.PullReceiver(bindAddress))
            {
                server.Start();
                for (int i = 0; i < 10; i++)
                {
                    client.Send(new PushMsg(string.Format("Test {0}", i)));
                }
                Thread.Sleep(1000);
            }
        }

        private static void DemoPublishSubscribe(string connectAddress, string bindAddress)
        {
            Console.WriteLine("\r\nPattern Publish/Subscribe :");
            using (var client1 = new Client.Subscriber(connectAddress, "group1"))
            using (var client2 = new Client.Subscriber(connectAddress, "group2"))
            using (var server = new Server.Publisher(bindAddress))
            {
                client1.Start();
                client2.Start();
                Thread.Sleep(500);
                for (int i = 0; i < 10; i++)
                {
                    server.Publish(string.Format("group{0}", i % 2 + 1), new PushMsg(string.Format("Test {0}", i)));
                }
                Thread.Sleep(1000);
            }
        }

        private static void DemoRequestReply(string connectAddress, string bindAddress)
        {
            Console.WriteLine("\r\nPattern Request/Reply:");
            using (var client = new Client.RequestClient(connectAddress))
            using (var server = new Server.ReplyReceiver(bindAddress))
            {
                server.Start();
                for (int i = 0; i < 3; i++)
                {
                    var response = client.Send(new RequestMsg(string.Format("#{0} What are you doing !?", i)));
                    Console.WriteLine("Response: {0}", response);
                }
                Thread.Sleep(1000);
            }
        }

        private static void DemoRequestReplyWithSingleClientContext(string connectAddress, string bindAddress)
        {
            Console.WriteLine("\r\nPattern Request/Reply (multiple requesters with single context factory):");
            using (var clientFactory = new Client.RequestClientFactory())
            using (var client1 = clientFactory.Create(connectAddress))
            using (var client2 = clientFactory.Create(connectAddress))
            using (var client3 = clientFactory.Create(connectAddress))
            using (var server = new Server.ReplyReceiver(bindAddress))
            {
                var clients = new IRequestClient[]
                {
                    client1,
                    client2, 
                    client3
                };
                for (int i = 0; i < clients.Length; i++)
                {
                    var client = clients[i];
                    var clientName = string.Format("Client{0}", i + 1);
                    Task.Factory.StartNew(() =>
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var response = client.Send(new RequestMsg(string.Format("#{0} {1}, what are you doing !?", j, clientName)));
                            Console.WriteLine("{1} response: {0}", response, clientName);
                        }
                    });
                }
                server.Start();
                Thread.Sleep(1000);
            }
        }

        private static int GetFreePort()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
    }
}