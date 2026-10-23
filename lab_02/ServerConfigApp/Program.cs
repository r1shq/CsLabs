using ServerConfigApp;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== ПРОВЕРКА КОНФИГУРАЦИИ СЕРВЕРА ===");
Console.WriteLine("Введите настройки игрового сервера.\n");

int players = ReadInt("Максимальное количество игроков: ");
int ram = ReadInt("Оперативная память (ГБ): ");
int port = ReadInt("Порт сервера: ");
bool online = ReadBool("Проверять подлинность аккаунтов");
bool pvp = ReadBool("Включить PvP");
bool pvpAllowed = ReadBool("Разрешено ли PvP правилами сервера");
bool whitelist = ReadBool("Включить белый список");
int whitelistPlayers = ReadInt("Количество игроков в белом списке: ");
bool isPublic = ReadBool("Сервер доступен из интернета");
bool ddos = ReadBool("Включена DDoS-защита");

var config = new ServerConfig(players, ram, port, online, pvp,
    pvpAllowed, whitelist, whitelistPlayers, isPublic, ddos);

var issues = ConfigValidator.Check(config);
var status = ConfigValidator.GetStatus(issues);

Console.WriteLine("\n=== РЕЗУЛЬТАТ ПРОВЕРКИ ===");
Console.WriteLine(status switch
{
    StartStatus.Ready => "Сервер готов к запуску.",
    StartStatus.Warning => "Сервер можно запустить, но необходимо предупредить администратора.",
    _ => "Запуск невозможен. Исправьте ошибки конфигурации."
});

if (issues.Count == 0)
{
    Console.WriteLine("Проблем не обнаружено.");
}
else
{
    Console.WriteLine($"Найдено проблем: {issues.Count}");
    foreach (var issue in issues)
    {
        string level = issue.Severity == Severity.Error ? "ОШИБКА" : "ПРЕДУПРЕЖДЕНИЕ";
        Console.WriteLine($"[{level}] {issue.Message}");
    }
}

static int ReadInt(string question)
{
    while (true)
    {
        Console.Write(question);
        if (int.TryParse(Console.ReadLine(), out int value))
            return value;
        Console.WriteLine("Введите целое число.");
    }
}

static bool ReadBool(string question)
{
    while (true)
    {
        Console.Write($"{question} (да/нет): ");
        string value = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (value is "да" or "д" or "yes" or "y" or "1") return true;
        if (value is "нет" or "н" or "no" or "n" or "0") return false;
        Console.WriteLine("Ответьте «да» или «нет».");
    }
}
