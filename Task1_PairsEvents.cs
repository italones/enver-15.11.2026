using System;
using System.IO;
using System.Threading;

public static class Task1_PairsEvents
{
    static event Action? GenerationCompleted;
    static readonly object fileLock = new object();
    static readonly string inputFile = "pairs.txt";
    static readonly string sumsFile  = "sums.txt";
    static readonly string prodsFile = "products.txt";

    public static void Run()
    {
        int pairsCount = 10;

        var generator  = new Thread(() => GeneratePairs(pairsCount));
        var summer     = new Thread(ComputeSum);
        var multiplier = new Thread(ComputeProduct);

        // Подписка ДО запуска генератора
        GenerationCompleted += summer.Start;
        GenerationCompleted += multiplier.Start;

        generator.Start();
        generator.Join();
        summer.Join();
        multiplier.Join();

        Console.WriteLine("Готово: pairs.txt, sums.txt, products.txt");
    }

    static void GeneratePairs(int n)
    {
        var rnd = new Random();
        using (var sw = new StreamWriter(inputFile))
        {
            for (int i = 0; i < n; i++)
            {
                int a = rnd.Next(1, 100);
                int b = rnd.Next(1, 100);
                sw.WriteLine($"{a} {b}");
                Thread.Sleep(50);
            }
        }
        Console.WriteLine("[Generator] Пары сгенерированы.");
        GenerationCompleted?.Invoke();
    }

    static void ComputeSum()
    {
        var lines = File.ReadAllLines(inputFile);
        lock (fileLock)
        using (var sw = new StreamWriter(sumsFile))
        {
            foreach (var line in lines)
            {
                var p = line.Split(' ');
                int a = int.Parse(p[0]), b = int.Parse(p[1]);
                sw.WriteLine($"{a} + {b} = {a + b}");
            }
        }
        Console.WriteLine("[Summer] Суммы записаны.");
    }

    static void ComputeProduct()
    {
        var lines = File.ReadAllLines(inputFile);
        lock (fileLock)
        using (var sw = new StreamWriter(prodsFile))
        {
            foreach (var line in lines)
            {
                var p = line.Split(' ');
                int a = int.Parse(p[0]), b = int.Parse(p[1]);
                sw.WriteLine($"{a} * {b} = {a * b}");
            }
        }
        Console.WriteLine("[Multiplier] Произведения записаны.");
    }
}