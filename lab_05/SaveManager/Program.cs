using System.Globalization;
using Newtonsoft.Json;

public class Program
{
    static string SavePath(string directory, SaveEntry entry)
    {
        string basePath = Path.GetFullPath(directory);
        string path = Path.GetFullPath(Path.Combine(basePath, entry.File ?? ""));
        if (string.IsNullOrWhiteSpace(entry.File) ||
            Path.GetFileName(entry.File) != entry.File ||
            !path.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Некорректное имя файла сохранения.");
        return path;
    }

    public static List<SaveEntry> LoadIndex(string indexPath)
    {
        if (!File.Exists(indexPath))
        {
            Console.WriteLine("Индекс не найден. Создан пустой список сохранений.");
            return new();
        }
        try
        {
            return JsonConvert.DeserializeObject<List<SaveEntry>>(File.ReadAllText(indexPath))
                ?? throw new JsonException("Пустой индекс JSON.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.WriteLine("Не удалось прочитать индекс: " + ex.Message);
            Console.WriteLine("Используется пустой список. Исходный индекс не изменён.");
            return new();
        }
    }

    public static SaveData? LoadSave(string savesDirectory, SaveEntry entry)
    {
        try
        {
            string path = SavePath(savesDirectory, entry);
            if (!File.Exists(path)) { Console.WriteLine("Файл сохранения не найден: " + entry.File); return null; }
            return JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(path))
                ?? throw new JsonException("Сохранение не содержит данных.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.WriteLine($"Ошибка чтения сохранения '{entry.Name}': {ex.Message}");
            return null;
        }
    }

    public static void PrintSave(SaveEntry entry, SaveData save)
    {
        Console.WriteLine($"Сохранение: {entry.Name}\nПерсонаж: {save.Character}\nКарта: {save.Map}");
        Console.WriteLine($"Позиция: X={save.Position?.X}, Y={save.Position?.Y}\nЗдоровье: {save.Health}");
        Console.WriteLine("Инвентарь: " + (save.Inventory?.Count > 0 ? string.Join(", ", save.Inventory) : "пусто"));
    }

    public static SaveEntry? FindByName(List<SaveEntry> entries, string name) =>
        entries.FirstOrDefault(e => string.Equals(e.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

    public static void SaveGame(string savesDirectory, SaveEntry entry, SaveData save)
    {
        Directory.CreateDirectory(savesDirectory);
        File.WriteAllText(SavePath(savesDirectory, entry), JsonConvert.SerializeObject(save, Formatting.Indented));
    }

    public static void SaveIndex(string indexPath, List<SaveEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(indexPath))!);
        File.WriteAllText(indexPath, JsonConvert.SerializeObject(entries, Formatting.Indented));
    }

    public static void CreateBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string source = SavePath(savesDirectory, entry);
        if (!File.Exists(source)) { Console.WriteLine("Резервная копия не создана: файл не найден."); return; }
        Directory.CreateDirectory(backupsDirectory);
        File.Copy(source, SavePath(backupsDirectory, entry), true);
        Console.WriteLine("Резервная копия создана.");
    }

    public static void RestoreBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string source = SavePath(backupsDirectory, entry);
        if (!File.Exists(source)) { Console.WriteLine("Резервная копия не найдена. Сохранение не изменено."); return; }
        Directory.CreateDirectory(savesDirectory);
        File.Copy(source, SavePath(savesDirectory, entry), true);
        Console.WriteLine("Сохранение восстановлено из копии.");
    }

    public static void DeleteSave(string savesDirectory, string indexPath, List<SaveEntry> entries, SaveEntry entry)
    {
        string path = SavePath(savesDirectory, entry);
        if (File.Exists(path)) File.Delete(path);
        else Console.WriteLine("Файл отсутствует; запись в индексе будет удалена.");
        entries.Remove(entry);
        SaveIndex(indexPath, entries);
        Console.WriteLine("Запись сохранения удалена из индекса.");
    }

    static string Ask(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? "";
    }

    static int AskInt(string prompt, int oldValue)
    {
        while (true)
        {
            string text = Ask($"{prompt} (сейчас {oldValue}, Enter — оставить): ");
            if (text == "") return oldValue;
            if (int.TryParse(text, out int result)) return result;
            Console.WriteLine("Введите целое число.");
        }
    }

    static void LogAction(string path, string name, string action)
    {
        try { File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {name} | {action}{Environment.NewLine}"); }
        catch (IOException ex) { Console.WriteLine("Не удалось записать журнал: " + ex.Message); }
        catch (UnauthorizedAccessException ex) { Console.WriteLine("Нет доступа к журналу: " + ex.Message); }
    }

    static SaveEntry? Choose(List<SaveEntry> entries)
    {
        string name = Ask("Введите имя сохранения: ");
        var entry = FindByName(entries, name);
        if (entry is null) Console.WriteLine("Сохранение не найдено.");
        return entry;
    }

    static void Edit(SaveData data)
    {
        Console.WriteLine("Enter оставляет значение без изменений.");
        string map = Ask($"Карта (сейчас {data.Map}): ");
        if (map.Length > 0) data.Map = map;
        data.Position ??= new Position();
        data.Position.X = AskInt("Координата X", data.Position.X);
        data.Position.Y = AskInt("Координата Y", data.Position.Y);
        data.Health = AskInt("Здоровье", data.Health);
        string items = Ask("Предметы через запятую (Enter — оставить, '-' — очистить): ");
        if (items == "-") data.Inventory = new();
        else if (items.Length > 0) data.Inventory = items.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public static void Main()
    {
        string root = AppContext.BaseDirectory;
        string saves = Path.Combine(root, "saves");
        string backups = Path.Combine(root, "backups");
        string index = Path.Combine(root, "index.json");
        string journal = Path.Combine(root, "operations.log");
        Directory.CreateDirectory(saves);
        Directory.CreateDirectory(backups);
        var entries = LoadIndex(index);

        while (true)
        {
            Console.WriteLine("\n1. Список сохранений\n2. Просмотр сохранения\n3. Изменить сохранение\n4. Создать копию\n5. Восстановить копию\n6. Удалить сохранение\n7. Выйти");
            switch (Ask("Выбор: "))
            {
                case "1":
                    if (entries.Count == 0) Console.WriteLine("Сохранений пока нет.");
                    foreach (var e in entries) Console.WriteLine($"{e.Name} | {e.File} | {e.UpdatedAt}");
                    break;
                case "2":
                {
                    var e = Choose(entries);
                    if (e is null) break;
                    var data = LoadSave(saves, e);
                    if (data is null) break;
                    PrintSave(e, data);
                    LogAction(journal, e.Name, "просмотр");
                    break;
                }
                case "3":
                {
                    var e = Choose(entries);
                    if (e is null) break;
                    var data = LoadSave(saves, e);
                    if (data is null) break;
                    Edit(data);
                    SaveGame(saves, e, data);
                    e.UpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    SaveIndex(index, entries);
                    LogAction(journal, e.Name, "изменение");
                    Console.WriteLine("Изменения сохранены.");
                    break;
                }
                case "4":
                {
                    var e = Choose(entries);
                    if (e is null) break;
                    if (!File.Exists(SavePath(saves, e))) { Console.WriteLine("Исходный файл отсутствует."); break; }
                    CreateBackup(saves, backups, e);
                    LogAction(journal, e.Name, "резервное копирование");
                    break;
                }
                case "5":
                {
                    var e = Choose(entries);
                    if (e is null) break;
                    if (!File.Exists(SavePath(backups, e))) { Console.WriteLine("Копия отсутствует."); break; }
                    RestoreBackup(saves, backups, e);
                    LogAction(journal, e.Name, "восстановление");
                    break;
                }
                case "6":
                {
                    var e = Choose(entries);
                    if (e is null) break;
                    DeleteSave(saves, index, entries, e);
                    LogAction(journal, e.Name, "удаление");
                    break;
                }
                case "7": return;
                default: Console.WriteLine("Выберите число от 1 до 7."); break;
            }
        }
    }
}
