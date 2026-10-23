using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== СОЗДАНИЕ ИГРОВОГО ПРОФИЛЯ ===");
        Console.WriteLine();

        Console.Write("Введите никнейм: ");
        string nickname = Console.ReadLine();

        Console.Write("Введите имя: ");
        string name = Console.ReadLine();

        Console.Write("Введите возраст: ");
        int age = int.Parse(Console.ReadLine());

        Console.Write("Введите любимую игру: ");
        string favoriteGame = Console.ReadLine();

        Console.Write("Введите уровень игрока: ");
        int level = int.Parse(Console.ReadLine());

        Console.Write("Введите количество часов в игре: ");
        int hours = int.Parse(Console.ReadLine());

        Console.WriteLine();
        Console.WriteLine("================================");
        Console.WriteLine("        ИГРОВОЙ ПРОФИЛЬ");
        Console.WriteLine("================================");
        Console.WriteLine($" Никнейм:        {nickname}");
        Console.WriteLine($" Имя:            {name}");
        Console.WriteLine($" Возраст:         {age}");
        Console.WriteLine($" Любимая игра:    {favoriteGame}");
        Console.WriteLine($" Уровень:         {level}");
        Console.WriteLine($" Часов в игре:    {hours}");
        Console.WriteLine("================================");
    }
}