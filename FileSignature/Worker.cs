using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace FileSignature
{
    internal class Worker
    {
        private Chunk chunk;
        private AutoResetEvent processFlag;
        public event EventHandler<WorkerEventArgs> WorkerResult;

        public Worker(Chunk chunk, AutoResetEvent flag)
        {
            this.chunk = chunk;
            processFlag = flag;
        }

        public void Start()
        {
            new Thread(() => {
                Run();
            }).Start();
        }

        void Run()
        {
            processFlag.WaitOne();
            WorkerEventArgs e = null;
            try
            {
                string hashResult = string.Empty;
        
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(chunk.Bytes, 0, chunk.Bytes.Length);
                    hashResult = string.Join("", hash.Select(h => h.ToString("X2")).ToArray());
                }
                e = new WorkerEventArgs(new Chunk { Hash = hashResult, Number = chunk.Number });
            }
            catch (Exception ex)
            {
                e = new WorkerEventArgs(null, "ERROR:" + ex.Message);
            }
            finally
            {
                OnWorkerResult(e);
                processFlag.Set();
            }
        }

        protected virtual void OnWorkerResult(WorkerEventArgs e)
        {
            WorkerResult?.Invoke(this, e);
        }
    }
}
