using System;
using System.Linq;

namespace DisruptorSamples.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var simpleQueue = new DisruptorSamples.Tests.Tests.SimpleQueueProcessorTest();

                Console.WriteLine("*** SimpleQueueProcessor ***");
                Console.WriteLine("Dummy processing:");
                Console.WriteLine("---------------------------");
                simpleQueue.TestWithZipDisabled();

                Console.WriteLine("---------------------------");
                Console.WriteLine("Zip processing:");
                Console.WriteLine("---------------------------");
                simpleQueue.TestWithZipEnabled();

                var multipleConsumers = new DisruptorSamples.Tests.Tests.MultipleConsumersProcessorTest();
                Console.WriteLine("*** MultipleConsumersProcessor ***");
                multipleConsumers.Test();

                var orderedConsumers = new DisruptorSamples.Tests.Tests.OrderedConsumersProcessorTests();
                Console.WriteLine("*** OrderedConsumersProcessor ***");
                orderedConsumers.Test();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Press a key to finish...");
                Console.ReadKey(true);
            }
        }
    }
}