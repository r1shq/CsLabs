using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerLogs;

public static class Program
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private static readonly Regex LinePattern = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(?<level>[^\]\r\n]+)\]\[(?<category>[^\]\r\n]+)\] (?<message>.*)$",
        RegexOptions.Compiled);

    public static LogEntry[] ParseLog(string[] lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var entries = new LogEntry[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i] ?? throw new FormatException($"Строка {i + 1} пуста (null).");
            Match match = LinePattern.Match(line);
            if (!match.Success || !DateTime.TryParseExact(
                match.Groups["timestamp"].Value, DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
            {
                throw new FormatException($"Неверный формат записи в строке {i + 1}: {line}");
            }

            entries[i] = new LogEntry
            {
                Timestamp = timestamp,
                Level = match.Groups["level"].Value,
                Category = match.Groups["category"].Value,
                Message = match.Groups["message"].Value
            };
        }

        return entries;
    }

    public static LogEntry[] FilterByDate(LogEntry[] entries, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return entries.Where(e => e.Timestamp.Date == date.Date).ToArray();
    }

    public static LogEntry[] FilterByLevel(LogEntry[] entries, string level)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(level);
        return entries.Where(e => string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public static LogEntry[] FilterByCategory(LogEntry[] entries, string category)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(category);
        return entries.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public static LogEntry[] Search(LogEntry[] entries, string text)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(text);
        return entries.Where(e => (e.Message ?? "").Contains(text, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public static int CountByLevel(LogEntry[] entries, string level)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(level);
        return entries.Count(e => string.Equals(e.Level, level, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetServerStatus(LogEntry[] entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Any(e => string.Equals(e.Level, "Fatal", StringComparison.OrdinalIgnoreCase)
            && string.Equals(e.Category, "Server", StringComparison.OrdinalIgnoreCase)))
            return "КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен";

        if (entries.Any(e => string.Equals(e.Level, "Error", StringComparison.OrdinalIgnoreCase)
            || string.Equals(e.Level, "Fatal", StringComparison.OrdinalIgnoreCase)))
            return "Есть ошибки: требуется проверка";

        return "Сервер работает штатно";
    }

    public static void ExportFiltered(string path, LogEntry[] entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(entries);

        string[] lines = entries.Select(e =>
            $"{e.Timestamp.ToString(DateFormat, CultureInfo.InvariantCulture)} [{e.Level}][{e.Category}] {e.Message}").ToArray();
        File.WriteAllLines(path, lines, new UTF8Encoding(false));
    }

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        string path = args.Length > 0 ? args[0] : "event_server.log";
        if (!File.Exists(path))
        {
            Console.WriteLine($"Журнал не найден: {Path.GetFullPath(path)}");
            Console.WriteLine("Поместите event_server.log в рабочую папку либо укажите путь первым аргументом.");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            LogEntry[] entries = ParseLog(lines);
            Console.WriteLine($"Загружено записей: {entries.Length}");
            Console.WriteLine($"Info: {CountByLevel(entries, "Info")}, Warning: {CountByLevel(entries, "Warning")}, " +
                $"Error: {CountByLevel(entries, "Error")}, Fatal: {CountByLevel(entries, "Fatal")}");
            Console.WriteLine(GetServerStatus(entries));

            if (args.Length > 1)
            {
                LogEntry[] found = Search(entries, args[1]);
                Console.WriteLine($"Записей с текстом «{args[1]}»: {found.Length}");
                foreach (LogEntry e in found.Take(10))
                    Console.WriteLine($"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{e.Level}][{e.Category}] {e.Message}");

                if (args.Length > 2)
                {
                    ExportFiltered(args[2], found);
                    Console.WriteLine($"Результат экспорта: {Path.GetFullPath(args[2])}");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
        {
            Console.WriteLine($"Не удалось обработать журнал: {ex.Message}");
        }
    }
}
