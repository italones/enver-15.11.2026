using System;
using System.Threading;

public static class Task2_BusTerminal
{
    static readonly object stopLock = new object();
    static int peopleAtStop = 0;
    static volatile bool dayOver = false;
    static event Action? BusArrived;

    public static void Run()
    {
        int[] busCapacities = { 30, 20, 40, 25 };
        const string busNumber = "175";

        BusArrived += () => { /* можно логировать */ };

        var peopleThread = new Thread(() =>
        {
            var rnd = new Random();
            for (int i = 0; i < 20; i++)
            {
                int arrived = rnd.Next(1, 15);
                lock (stopLock)
                {
                    peopleAtStop += arrived;
                    Console.WriteLine($"[Люди] +{arrived}, на остановке {peopleAtStop}");
                }
                Thread.Sleep(rnd.Next(100, 300));
            }
            dayOver = true;
        });

        var busThreads = new Thread[busCapacities.Length];
        for (int i = 0; i < busCapacities.Length; i++)
        {
            int cap = busCapacities[i];
            busThreads[i] = new Thread(() => RunBus(busNumber, cap));
            busThreads[i].Start();
        }

        peopleThread.Start();
        peopleThread.Join();
        foreach (var t in busThreads) t.Join(TimeSpan.FromSeconds(5));

        lock (stopLock)
            Console.WriteLine($"[Итог] На остановке осталось {peopleAtStop}");
    }

    static void RunBus(string number, int capacity)
    {
        var rnd = new Random(Environment.CurrentManagedThreadId);
        while (!dayOver)
        {
            Thread.Sleep(rnd.Next(300, 700));
            lock (stopLock)
            {
                int taken = Math.Min(capacity, peopleAtStop);
                peopleAtStop -= taken;
                Console.WriteLine($"[Автобус {number}] Забрал {taken}/{capacity}, на остановке {peopleAtStop}");
            }
            BusArrived?.Invoke();
        }
    }
}