using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;

namespace ZmqServiceBus.Tests
{
    [TestFixture]
    public sealed class IpcServiceBusTest
    {
        private static readonly ITraceWriter Logger = new DebugTraceWriter();

        private const bool VerboseLog = false;
        private IpcEventProxy _proxy;
        private int _pubPort, _subPort;

        [Test]
        [Repeat(10)]
        public void SingleEvent()
        {
            SetUp();
            try
            {
                using (var bus = new IpcServiceBus(string.Format("tcp://localhost:{0}", _pubPort), string.Format("tcp://localhost:{0}", _subPort), VerboseLog, Logger))
                    using (var signal = new ManualResetEvent(false))
                    {
                        const long testEventCode = 1;
                        IPublisher publisher = null;
                        ISubscriber subscriber = bus.CreateSubscriber(testEventCode);
                        try
                        {
                            TestEvent testEventReceived = null;
                            int messageReceivedCount = 0;
                            subscriber.OnMessage += (o, e) =>
                            {
                                Logger.Debug("Test event received.");
                                messageReceivedCount++;
                                Assert.AreEqual(testEventCode, e.EventCode, "Event code received unexpected.");
                                testEventReceived = e.GetMessage<TestEvent>();
                                signal.Set();
                            };

                            publisher = bus.CreatePublisher();
                            var eventToSend = new TestEvent() { SendTimeTicks = DateTime.UtcNow.Ticks };
                            
                            Stopwatch watch = Stopwatch.StartNew();
                            publisher.Publish(testEventCode, eventToSend);
                            Assert.IsTrue(signal.WaitOne(300), "No event received.");
                            watch.Stop();
                            Logger.Debug("Delay between send/rec: {0:F02} ms. (ticks: {1}).", watch.ElapsedMilliseconds, watch.ElapsedTicks);
                            Assert.AreEqual(1, messageReceivedCount, "messageReceivedCount unexpected.");
                            Assert.AreEqual(eventToSend.SendTimeTicks, testEventReceived.SendTimeTicks, "testEventReceived.SendTimeTicks unexpected.");
                        }
                        finally
                        {
                            bus.Release(subscriber);
                            if (publisher != null)
                                bus.Release(publisher);
                        }
                    }
            }
            finally
            {
                TearDown();
            }
        }

        [TestCase(1000)]
        [TestCase(5000)]
        [TestCase(10000)]
        public void BatchOfEvents(int eventCount)
        {
            SetUp();
            try
            {
                using (var bus = new IpcServiceBus(string.Format("tcp://localhost:{0}", _pubPort), string.Format("tcp://localhost:{0}", _subPort), VerboseLog, Logger))
                    using (var signal = new ManualResetEvent(false))
                    {
                        const long testEventCode = 1;
                        IPublisher publisher = null;
                        ISubscriber subscriber = bus.CreateSubscriber(testEventCode);
                        try
                        {
                            int messageReceivedCount = 0;
                            subscriber.OnMessage += (o, e) =>
                            {
                                if (eventCount == ++messageReceivedCount)
                                    signal.Set();
                            };

                            publisher = bus.CreatePublisher();
                            Thread.Sleep(1000);
                            var eventToSend = new TestEvent() { SendTimeTicks = DateTime.UtcNow.Ticks };

                            Stopwatch watch = Stopwatch.StartNew();
                            for (int i = 0; i < eventCount; i++)
                            {
                                publisher.Publish(testEventCode, eventToSend);
                            }
                            signal.WaitOne(30000);
                            watch.Stop();
                            Assert.AreEqual(eventCount, messageReceivedCount, "messageReceivedCount unexpected.");
                            Logger.Debug("Delay between first send/last rec: {0:F02} ms. (ticks: {1}).", watch.ElapsedMilliseconds, watch.ElapsedTicks);
                        }
                        finally
                        {
                            bus.Release(subscriber);
                            if (publisher != null)
                                bus.Release(publisher);
                        }
                    }
            }
            finally
            {
                TearDown();
            }
        }

        private static int[] GetAvailablePort(int count)
        {
            var ports = new int[count];
            for (int i = 0; i < count; i++)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                    ports[i] = ((IPEndPoint)socket.LocalEndPoint).Port;
                }
            }
            return ports;
        }

        private void SetUp()
        {
            int[] ports = GetAvailablePort(2);
            _pubPort = ports[0];
            _subPort = ports[1];
            _proxy = new IpcEventProxy(string.Format("tcp://*:{0}", _subPort), string.Format("tcp://*:{0}", _pubPort), VerboseLog, Logger);
        }

        private void TearDown()
        {
            _proxy.Dispose();
        }
    }
}