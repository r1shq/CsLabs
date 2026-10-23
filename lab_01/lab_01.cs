using System;

class Program
{
    static void Main()
    {
        Console.Write("Название сервера: ");
        string name = Console.ReadLine() ?? "";

        Console.Write("Сервер включён (true/false): ");
        bool isOnline = bool.Parse(Console.ReadLine()!);

        Console.Write("Максимальное количество игроков: ");
        int maxPlayers = int.Parse(Console.ReadLine()!);

        Console.Write("Текущее количество игроков: ");
        int players = int.Parse(Console.ReadLine()!);

        Console.Write("Всего оперативной памяти (МБ): ");
        int totalRam = int.Parse(Console.ReadLine()!);

        Console.Write("Занято оперативной памяти (МБ): ");
        int usedRam = int.Parse(Console.ReadLine()!);

        Console.Write("Пинг (мс): ");
        int ping = int.Parse(Console.ReadLine()!);

        if (maxPlayers <= 0 || players < 0 || players > maxPlayers ||
            totalRam <= 0 || usedRam < 0 || usedRam > totalRam ||
            ping < 0)
        {
            Console.WriteLine("Ошибка: указаны некорректные значения.");
            return;
        }

        int freeSlots = maxPlayers - players;
        int freeRam = totalRam - usedRam;

        double occupancy = players * 100.0 / maxPlayers;
        double ramUsage = usedRam * 100.0 / totalRam;

        bool isReady = isOnline && freeSlots > 0 &&
                       ramUsage < 90 && ping <= 100;

        Console.WriteLine("\n========== СВОДКА СЕРВЕРА ==========");
        Console.WriteLine($"Название:           {name}");
        Console.WriteLine($"Состояние:          {(isOnline ? "Включён" : "Выключен")}");
        Console.WriteLine($"Игроки:             {players} / {maxPlayers}");
        Console.WriteLine($"Заполненность:      {occupancy:F1}%");
        Console.WriteLine($"Свободные места:    {freeSlots}");
        Console.WriteLine($"Память занята:      {usedRam} / {totalRam} МБ");
        Console.WriteLine($"Загрузка памяти:    {ramUsage:F1}%");
        Console.WriteLine($"Свободная память:   {freeRam} МБ");
        Console.WriteLine($"Пинг:               {ping} мс");

        Console.WriteLine(
            $"\nИтог: {(isReady ? "Готов принимать игроков" : "Требует внимания")}");

        if (!isOnline)
            Console.WriteLine("— Сервер выключен.");

        if (freeSlots == 0)
            Console.WriteLine("— Нет свободных мест.");

        if (ramUsage >= 90)
            Console.WriteLine("— Занято 90% памяти или больше.");

        if (ping > 100)
            Console.WriteLine("— Высокий пинг.");
    }
}