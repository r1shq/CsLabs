using NUnit.Framework;
using ServerLogs;

namespace ServerLogTests;

public class ExampleTests
{
    private readonly string[] sample =
    {
        "2026-09-01 10:00:00.001 [Info][Server] Starting",
        "2026-09-02 11:00:00.100 [Warning][Combat] High load",
        "2026-09-02 12:00:00.200 [Error][Database] Connection failed",
        "2026-09-02 13:00:00.300 [Fatal][Server] Database unavailable"
    };

    [Test]
    public void ParseLog_ParsesFields()
    {
        var log = Program.ParseLog(sample);
        Assert.That(log.Length, Is.EqualTo(4));
        Assert.That(log[0].Timestamp, Is.EqualTo(new DateTime(2026, 9, 1, 10, 0, 0, 1)));
        Assert.That(log[0].Level, Is.EqualTo("Info"));
        Assert.That(log[0].Category, Is.EqualTo("Server"));
        Assert.That(log[0].Message, Is.EqualTo("Starting"));
    }

    [Test]
    public void ParseLog_InvalidDate_Throws() =>
        Assert.Throws<FormatException>(() => Program.ParseLog(new[] {"2026-02-30 10:00:00.000 [Info][Server] Wrong date"}));

    [Test]
    public void FilterByDate_FindsOneDay() =>
        Assert.That(Program.FilterByDate(Program.ParseLog(sample), new DateTime(2026, 9, 2)).Length, Is.EqualTo(3));

    [Test]
    public void FilterByLevel_IgnoresCase() =>
        Assert.That(Program.FilterByLevel(Program.ParseLog(sample), "warning").Length, Is.EqualTo(1));

    [Test]
    public void FilterByCategory_FindsDatabase() =>
        Assert.That(Program.FilterByCategory(Program.ParseLog(sample), "Database").Length, Is.EqualTo(1));

    [Test]
    public void Search_IgnoresCase() =>
        Assert.That(Program.Search(Program.ParseLog(sample), "DATABASE").Length, Is.EqualTo(1));

    [Test]
    public void CountByLevel_CountsErrors() =>
        Assert.That(Program.CountByLevel(Program.ParseLog(sample), "Error"), Is.EqualTo(1));

    [Test]
    public void Status_FatalServer_IsCritical() =>
        Assert.That(Program.GetServerStatus(Program.ParseLog(sample)), Is.EqualTo("КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен"));

    [Test]
    public void Status_Error_RequiresCheck() =>
        Assert.That(Program.GetServerStatus(Program.ParseLog(sample[..3])), Is.EqualTo("Есть ошибки: требуется проверка"));

    [Test]
    public void Status_WithoutErrors_IsHealthy() =>
        Assert.That(Program.GetServerStatus(Program.ParseLog(sample[..2])), Is.EqualTo("Сервер работает штатно"));

    [Test]
    public void EmptyLog_IsHealthy() =>
        Assert.That(Program.GetServerStatus(Array.Empty<LogEntry>()), Is.EqualTo("Сервер работает штатно"));

    [Test]
    public void ExportFiltered_PreservesFormat()
    {
        string path = Path.GetTempFileName();
        try
        {
            Program.ExportFiltered(path, Program.FilterByLevel(Program.ParseLog(sample), "Info"));
            Assert.That(File.ReadAllLines(path), Is.EqualTo(new[] { sample[0] }));
        }
        finally { File.Delete(path); }
    }
}
