using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace FileSignature
{
    internal class FileReader
    {
        private readonly string _filePath;
        private readonly int _chunk;
        private readonly long _fileSize;
        private readonly long _chunkAmount;
        private readonly int _threadLimit;
        private SHA256 _sha256Hash;
        private MemoryMappedFile _mmf;

        private readonly object lockObject = new();

        internal FileReader(string filePath, int chunk)
        {
            _filePath = filePath;
            _chunk = chunk;
            _fileSize = new FileInfo(filePath).Length;
            _chunkAmount = _fileSize / chunk;
            _threadLimit = 16; //?
        }

        public void Process()
        {
            var listThread = new List<Thread>();
          
            try
            {
                _sha256Hash = SHA256.Create();
                _mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open);

                var customThreadPool = new CustomThreadPool<int>(_threadLimit);

                customThreadPool.StartWorkers(value =>
                {
                    Work(value);
                });

           
                for (int z = 0; z <= _chunkAmount; z++)
                {
                    customThreadPool.Enqueue(z);
                }

                customThreadPool.CompleteAdding();

                customThreadPool.Await();               
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

        private void Work(object data)
        {
            int i = (int)data;

            var leftTotalLegth = _fileSize - (i * _chunk);
            int leghtLeft = (int)Math.Min(_chunk, leftTotalLegth);

            if (leghtLeft > 0)
            {           
                 var bytes = ReadChunkFromMemoryMappedFile(_mmf, _chunk * i, leghtLeft);
            
                lock (lockObject) // not parallel code((
                {
                    var hash = GetHash(_sha256Hash, bytes);

                    Console.WriteLine($"{i} - {hash}");
                }                                        
            }
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

        private void Dispose()
        {
            _sha256Hash?.Dispose();
            _mmf?.Dispose();
        }
    }
}
