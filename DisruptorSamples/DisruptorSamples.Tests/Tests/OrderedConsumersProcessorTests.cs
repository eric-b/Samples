using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DisruptorSamples.Tests.Tests
{
    [TestFixture]
    public class OrderedConsumersProcessorTests
    {
        [Test]
        public void Test()
        {
            const int count = 10;
            string[] filesSample = new string[count];
            for (int i = 0; i < count; i++)
            {
                string path = Path.GetTempFileName();
                File.WriteAllText(path, string.Format("Test n°{0}\r\n{1}", i + 1, DateTime.Now), Encoding.UTF8);
                filesSample[i] = path;
            }

            var options = new OrderedConsumersProcessor.OrderedConsumersProcessorOptions();
            using (var proc = new OrderedConsumersProcessor.OrderedConsumersProcessor(options))
            {
                proc.Start();
                foreach (var path in filesSample)
                {
                    Console.WriteLine("{1:HH:mm:ss.fff}: Publish file: {0}.", path, DateTime.Now);
                    proc.Publish(path);
                }
            }
        }
    }
}