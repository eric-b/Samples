using System;
using System.IO;
using System.Linq;
using System.Text;
using DisruptorSamples.SimpleQueue;
using NUnit.Framework;

namespace DisruptorSamples.Tests.Tests
{
    [TestFixture]
    public class SimpleQueueProcessorTest
    {
        [Test]
        public void TestWithZipEnabled()
        {
            const int count = 10;
            string[] filesSample = new string[count];
            for (int i = 0; i < count; i++)
            {
                string path = Path.GetTempFileName();
                File.WriteAllText(path, string.Format("Test n°{0}\r\n{1}", i + 1, DateTime.Now), Encoding.UTF8);
                filesSample[i] = path;
            }

            var options = new SimpleQueueProcessorOptions();
            using (var proc = new SimpleQueueProcessor(options))
            {
                proc.Start();
                foreach (var path in filesSample)
                {
                    Console.WriteLine("{1:HH:mm:ss.fff}: Publishing file: {0}.", path, DateTime.Now);
                    proc.Publish(path);
                }
            }
        }

        [Test]
        public void TestWithZipDisabled()
        {
            const int count = 10;
            string[] filesSample = new string[count];
            for (int i = 0; i < count; i++)
            {
                string path = Path.GetTempFileName();
                File.WriteAllText(path, string.Format("Test n°{0}\r\n{1}", i + 1, DateTime.Now), Encoding.UTF8);
                filesSample[i] = path;
            }

            var options = new SimpleQueueProcessorOptions();
            options.EnableZip = false;
            using (var proc = new SimpleQueueProcessor(options))
            {
                proc.Start();
                foreach (var path in filesSample)
                {
                    proc.Publish(path);
                    Console.WriteLine("{1:HH:mm:ss.fff}: File published: {0}.", path, DateTime.Now);
                }
            }
        }
    }
}