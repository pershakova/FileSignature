using System;

namespace FileSignature
{
   internal class WorkerEventArgs : EventArgs
    {
        public string Error { get; private set; }
        public Chunk Chunk { get; private set; }

        public WorkerEventArgs(Chunk chunk)
            : this(chunk, null)
        {
        }

        public WorkerEventArgs(string error)
            : this(null, error)
        {
        }

        public WorkerEventArgs(Chunk chunk, string error)
        {
            Chunk = chunk;
            Error = error;
        }
    }
}
