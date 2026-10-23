using System;
using System.Globalization;
using System.IO;
using System.Text;

public static class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        string path = Path.Combine(AppContext.BaseDirectory, "event_server.log");
        if (!File.Exists(path))
        {
            Console.WriteLine("Не найден event_server.log. Положите его рядом с программой или в папку проекта.");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string report = BuildReport(lines);
        Console.WriteLine(report);
    }

    public static string BuildReport(string[] lines)
    {
        const string eventName = "Восстание Ледяного Пламени";
        string startMarker = "Событие началось: " + eventName;
        string endMarker = "Событие \"" + eventName + "\" закрыто";
        int start = -1;
        int end = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            if (start == -1 && lines[i].Contains(startMarker, StringComparison.Ordinal))
                start = i;
            if (start != -1 && lines[i].Contains(endMarker, StringComparison.Ordinal))
            {
                end = i;
                break;
            }
        }

        if (start == -1 || end == -1)
            return "Ошибка: в журнале не найдены начало и завершение события " + eventName + ".";

        string date = "не найдена";
        string winner = "не найден";
        string winnerPoints = "не найдены";
        string item = "не найден";
        string consolationTeam = "не найдена";
        string consolationPoints = "не найдены";
        string bestPlayer = "не найден";
        int bestKills = -1;
        int warnings = 0;
        int errors = 0;

        for (int i = start; i <= end; i++)
        {
            string line = lines[i];
            int levelStart = line.IndexOf('[', StringComparison.Ordinal);
            if (levelStart < 0) continue;

            if (i == start && line.Length >= 23 &&
                DateTime.TryParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
                date = timestamp.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

            int levelEnd = line.IndexOf(']', levelStart + 1);
            int categoryStart = line.IndexOf('[', levelEnd + 1);
            int categoryEnd = line.IndexOf(']', categoryStart + 1);
            if (levelEnd < 0 || categoryStart < 0 || categoryEnd < 0) continue;

            string level = line.Substring(levelStart + 1, levelEnd - levelStart - 1);
            string category = line.Substring(categoryStart + 1, categoryEnd - categoryStart - 1);
            string message = line.Substring(categoryEnd + 1).Trim();

            if (level == "Warning") warnings++;
            if (level == "Error") errors++;

            if (category == "Reward")
            {
                string winnerMarker = " объявлены победителями события";
                string pointsMarker = " получили ";
                string consolationMarker = " получили утешительную награду: ";

                int position = message.IndexOf(winnerMarker, StringComparison.Ordinal);
                if (position > 0)
                    winner = message.Substring(0, position);

                position = message.IndexOf(consolationMarker, StringComparison.Ordinal);
                if (position > 0)
                {
                    consolationTeam = message.Substring(0, position);
                    string tail = message.Substring(position + consolationMarker.Length);
                    int separator = tail.IndexOf(' ');
                    if (separator > 0) consolationPoints = tail.Substring(0, separator);
                }
                else
                {
                    position = message.IndexOf(pointsMarker, StringComparison.Ordinal);
                    if (position > 0 && message.EndsWith(" очков события", StringComparison.Ordinal))
                    {
                        string team = message.Substring(0, position);
                        string tail = message.Substring(position + pointsMarker.Length);
                        int separator = tail.IndexOf(' ');
                        if (separator > 0 && (winner == "не найден" || team == winner))
                            winnerPoints = tail.Substring(0, separator);
                    }
                }
            }
            else if (category == "Loot")
            {
                const string marker = " получили ивентовый предмет: ";
                int position = message.IndexOf(marker, StringComparison.Ordinal);
                if (position >= 0)
                    item = message.Substring(position + marker.Length);
            }
            else if (category == "Statistics")
            {
                const string marker = " победил ";
                int position = message.IndexOf(marker, StringComparison.Ordinal);
                if (message.StartsWith("Игрок ", StringComparison.Ordinal) && position > 6)
                {
                    string player = message.Substring(6, position - 6);
                    string tail = message.Substring(position + marker.Length);
                    int separator = tail.IndexOf(' ');
                    if (separator > 0 && int.TryParse(tail.Substring(0, separator), out int kills) && kills > bestKills)
                    {
                        bestKills = kills;
                        bestPlayer = player;
                    }
                }
            }
        }

        for (int i = start; i <= end; i++)
        {
            string line = lines[i];
            string marker = winner + " получили ";
            int position = line.IndexOf(marker, StringComparison.Ordinal);
            if (winner != "не найден" && position >= 0 && line.EndsWith(" очков события", StringComparison.Ordinal))
            {
                string tail = line.Substring(position + marker.Length);
                int separator = tail.IndexOf(' ');
                if (separator > 0) winnerPoints = tail.Substring(0, separator);
            }
        }

        return "# Итоги события: " + eventName + "\n\n" +
               "Дата: " + date + "\n" +
               "Победитель: " + winner + "\n" +
               "Очки победителя: " + winnerPoints + "\n" +
               "Ивентовый предмет: " + item + "\n" +
               "Утешительная награда " + consolationTeam + ": " + consolationPoints + " очков\n" +
               "Предупреждений во время события: " + warnings + "\n" +
               "Ошибок во время события: " + errors + "\n" +
               "Лучший игрок: " + bestPlayer + (bestKills >= 0 ? " (побеждено врагов: " + bestKills + ")" : "");
    }
}
