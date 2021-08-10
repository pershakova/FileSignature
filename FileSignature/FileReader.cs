using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FileSignature
{
    internal class FileReader : IDisposable
    {
        private readonly string _filePath;
        private readonly int _chunk;
        private readonly long _fileSize;
        private readonly int _chunkAmount;
        private readonly int _processorCount;
        private MemoryMappedFile _mmf;

        internal FileReader(string filePath, int chunk)
        {
            _filePath = filePath;
            _chunk = chunk;
            _fileSize = new FileInfo(filePath).Length;
            _chunkAmount = (int)_fileSize / chunk;
            _processorCount = Environment.ProcessorCount;       
        }

        public void Process()
        {
            var events = new AutoResetEvent[_processorCount];

            int totalChunks =  ((_fileSize % _chunk) == 0)? _chunkAmount : _chunkAmount + 1;

            int counter = totalChunks;
            int m = 0;
            try
            {
                using (_mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open))
                {
                    for (int j = 0; j < Math.Ceiling((double)totalChunks / _processorCount); j++)
                    {
                        for (int i = 0; i < _processorCount; i++)
                        {
                            byte[] data = new byte[_chunk];
                            counter--;
                            data = ReadFile(m);
                            m++;
                            
                            var block = new Chunk { Bytes = data, Number = totalChunks - counter };
                            events[i] = new AutoResetEvent(false);
                            var worker = new Worker(block, events[i]);
                            worker.WorkerResult += new EventHandler<WorkerEventArgs>(worker_WorkerResult);
                            worker.Start();
                            events[i].Set();

                            if (counter == 0)
                            {
                                break;
                            }
                        }
                      
                        WaitHandle.WaitAll(events);
                    }

                }
                Console.ReadLine();
            }
            catch
            {
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        private byte[] ReadFile(int i)
        {
            var leftTotalLegth = _fileSize - (i * _chunk);
            int leghtLeft = (int)Math.Min(_chunk, leftTotalLegth);

            return ReadChunkFromMemoryMappedFile(_mmf, _chunk * i, leghtLeft);            
        }

        private string GetHash(HashAlgorithm hashAlgorithm, byte[] input)
        {
            var data = hashAlgorithm.ComputeHash(input);

            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        private byte[] ReadChunkFromMemoryMappedFile(MemoryMappedFile mmf, int offset, int length)
        {
            using (MemoryMappedViewAccessor mmfReader = mmf.CreateViewAccessor(offset, length))
            {
                byte[] buffer = new byte[length];
                mmfReader.ReadArray(0, buffer, 0, length);
                return buffer;
            }
        }

        public void Dispose()
        {
            _mmf?.Dispose();
        }

        static void worker_WorkerResult(object sender, WorkerEventArgs e)
        {
            if (e.Chunk != null)
            {
                Console.WriteLine($"{e.Chunk.Number} - {e.Chunk.Hash}");
            }
            if (e.Error != null)
            {
                Console.WriteLine(e.Error);
            }
        }
    }
}
