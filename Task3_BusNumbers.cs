using System;
using System.Collections.Concurrent;
using System.Threading;

public static class Task3_BusNumbers
{
    static readonly ConcurrentQueue<string> passengers = new();
    static readonly string[] busNumbers = { "175", "42", "7" };
    static volatile bool dayOver = false;

    public static void Run()
    {
        var peopleThread = new Thread(() =>
        {
            var rnd = new Random();
            for (int i = 0; i < 30; i++)
            {
                string wish = busNumbers[rnd.Next(busNumbers.Length)];
                passengers.Enqueue(wish);
                Console.WriteLine($"[Человек] Хочет {wish}. В очереди: {passengers.Count}");
                Thread.Sleep(rnd.Next(80, 200));
            }
            dayOver = true;
        });

        int[] capacities = { 15, 10, 20 };
        var busThreads = new Thread[busNumbers.Length];
        for (int i = 0; i < busNumbers.Length; i++)
        {
            string n = busNumbers[i];
            int cap = capacities[i];
            busThreads[i] = new Thread(() => RunBus(n, cap));
            busThreads[i].Start();
        }

        peopleThread.Start();
        peopleThread.Join();
        foreach (var t in busThreads) t.Join(TimeSpan.FromSeconds(5));

        Console.WriteLine($"[Итог] Осталось пассажиров: {passengers.Count}");
    }

    static void RunBus(string number, int capacity)
    {
        var rnd = new Random(Environment.CurrentManagedThreadId);
        while (!dayOver)
        {
            Thread.Sleep(rnd.Next(300, 800));

            int taken = 0, total = passengers.Count;
            var rejected = new ConcurrentQueue<string>();
            for (int i = 0; i < total && taken < capacity; i++)
            {
                if (passengers.TryDequeue(out var wish))
                {
                    if (wish == number) taken++;
                    else rejected.Enqueue(wish);
                }
            }
            while (rejected.TryDequeue(out var w)) passengers.Enqueue(w);

            Console.WriteLine($"[Автобус {number}] Забрал {taken}/{capacity}, в очереди {passengers.Count}");
        }
    }
}