using System;
using System.Collections.Concurrent;
using System.Threading;

public static class Task4_NonTerminalStop
{
    static readonly ConcurrentQueue<string> passengers = new();
    static readonly string[] busNumbers = { "175", "42", "7" };
    static readonly object consoleLock = new object();
    static volatile bool dayOver = false;

    public static void Run()
    {
        var peopleThread = new Thread(() =>
        {
            var rnd = new Random();
            for (int i = 0; i < 40; i++)
            {
                string wish = busNumbers[rnd.Next(busNumbers.Length)];
                passengers.Enqueue(wish);
                lock (consoleLock)
                    Console.WriteLine($"[Человек] Хочет {wish}. В очереди: {passengers.Count}");
                Thread.Sleep(rnd.Next(60, 150));
            }
            dayOver = true;
        });

        int[] capacities = { 15, 10, 20 };
        var busThreads = new Thread[busNumbers.Length];
        for (int i = 0; i < busNumbers.Length; i++)
        {
            string n = busNumbers[i];
            int cap = capacities[i];
            int onboard = new Random(i + 1).Next(0, cap); // приезжает с пассажирами
            busThreads[i] = new Thread(() => RunBus(n, cap, onboard));
            busThreads[i].Start();
        }

        peopleThread.Start();
        peopleThread.Join();
        foreach (var t in busThreads) t.Join(TimeSpan.FromSeconds(5));
    }

    static void RunBus(string number, int capacity, int onboard)
    {
        var rnd = new Random(Environment.CurrentManagedThreadId);
        while (!dayOver)
        {
            Thread.Sleep(rnd.Next(400, 900));

            int off = rnd.Next(0, onboard + 1);
            onboard -= off;

            int freeSeats = capacity - onboard;
            int taken = 0, total = passengers.Count;
            var rejected = new ConcurrentQueue<string>();
            for (int i = 0; i < total && taken < freeSeats; i++)
            {
                if (passengers.TryDequeue(out var wish))
                {
                    if (wish == number) taken++;
                    else rejected.Enqueue(wish);
                }
            }
            while (rejected.TryDequeue(out var w)) passengers.Enqueue(w);

            onboard += taken;

            lock (consoleLock)
                Console.WriteLine($"[Автобус {number}] Вышло {off}, село {taken}, на борту {onboard}/{capacity}, в очереди {passengers.Count}");
        }
    }
}