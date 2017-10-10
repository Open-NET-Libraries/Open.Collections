using System;
using System.Collections.Concurrent;
using Open.Collections;
using System.Threading;

namespace ObjectPool_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var pool = new ObjectPool<object>(20, () => new object());
            var tank = new ConcurrentBag<object>();

            int count = 0;
            while (true)
            {
                count++;
                tank.Add(pool.Take());
                count++;
                tank.Add(pool.Take());
                foreach (var o in tank.TryTakeWhile(c => c.Count > 30))
                {
                    var x = o;
                    pool.Give(ref x);
                }

                Console.WriteLine("{0}: {1}, {2}",count, pool.Count, tank.Count);

                if (count % 40 == 0)
                    Thread.Sleep(6000);
            }
        }
    }
}
