using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FileSignature
{
    internal class CustomThreadPool<T> : IDisposable
    {
        private readonly BlockingCollection<T> queue;
        private readonly List<Thread> threadList;
      
        private readonly int maxWorkers;
        private bool wasShutDown;

        private int waitingUnits;

        public CustomThreadPool(int maxWorkers)
        {
            this.maxWorkers = maxWorkers;
            threadList = new List<Thread>();
            queue = new BlockingCollection<T>();
        }
        public void Enqueue(T value)
        {
            queue.Add(value);
            waitingUnits++;
        }
        
        public void CompleteAdding()
        {
            queue.CompleteAdding();
        }

        public void StartWorkers(Action<T> worker)
        {
            for (int i = 0; i < maxWorkers; i++)
            {
                threadList.Add(new Thread(() =>
                {
                    while (waitingUnits > 0 || !queue.IsAddingCompleted)
                    {
                        var value = queue.Take();
                        waitingUnits--;
                        worker(value);
                    }                   
                }
                ));
            }

            foreach (var thread in threadList)
            {
                thread.Start();
            }
        }

        public void Await()
        {
            Shutdown();
        }

        private void Shutdown()
        {
            wasShutDown = true;
            foreach (var thread in threadList)
            {
                thread.Join();
            }
        }

        public void Dispose()
        {
            if (!wasShutDown)
            {
                queue.CompleteAdding();
                Shutdown();
            }
        }
    }
}
