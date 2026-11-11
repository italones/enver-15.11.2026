Console.WriteLine("Выберите задание (1-4):");
switch (Console.ReadLine())
{
    case "1": Task1_PairsEvents.Run();        break;
    case "2": Task2_BusTerminal.Run();        break;
    case "3": Task3_BusNumbers.Run();         break;
    case "4": Task4_NonTerminalStop.Run();    break;
    default:  Console.WriteLine("Нет такого задания"); break;
}