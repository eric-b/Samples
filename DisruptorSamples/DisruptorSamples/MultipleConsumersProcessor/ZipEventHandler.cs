using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Disruptor;

namespace DisruptorSamples.MultipleConsumersProcessor
{
    internal sealed class ZipEventHandler : IEventHandler<Event>
    {
        void IEventHandler<Event>.OnNext(Event data, long sequence, bool endOfBatch)
        {
            try
            {
                var zipPath = string.Format("{0}.zip", data.Filepath);
                using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(
                        data.Filepath,
                        Path.GetFileName(data.Filepath),
                        CompressionLevel.Optimal);
                }
                Console.WriteLine(string.Format("{1:HH:mm:ss.fff}: ZIP created: {0}", zipPath, DateTime.Now));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}