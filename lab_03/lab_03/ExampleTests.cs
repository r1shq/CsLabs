using System;
using System.IO;
using NUnit.Framework;

[TestFixture]
public class ExampleTests
{
    private static string Report() => Program.BuildReport(File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "event_server.log")));

    [Test]
    public void RealLog_ReportHasExpectedDetails()
    {
        string report = Report();
        StringAssertContains(report, "Дата: 01.09.2026");
        StringAssertContains(report, "Победитель: Ночные совы");
        StringAssertContains(report, "Очки победителя: 2500");
        StringAssertContains(report, "Ивентовый предмет: Сердце Эмберфанга");
        StringAssertContains(report, "Утешительная награда Железных волков: 800 очков");
        StringAssertContains(report, "Предупреждений во время события: 6");
        StringAssertContains(report, "Ошибок во время события: 1");
        StringAssertContains(report, "Лучший игрок: ВоронФокс (побеждено врагов: 14)");
    }

    [Test]
    public void MissingEvent_ReturnsError()
    {
        Assert.That(Program.BuildReport(new[] { "some other event" }), Does.StartWith("Ошибка:"));
    }

    [Test]
    public void RecordsOutsideEvent_AreIgnored()
    {
        string[] lines = {
            "2026-09-01 12:00:00.000 [Warning][Server] До события",
            "2026-09-01 12:01:00.000 [Info][Event] Событие началось: Восстание Ледяного Пламени",
            "2026-09-01 12:02:00.000 [Error][Server] Внутри события",
            "2026-09-01 12:03:00.000 [Info][Server] Событие \"Восстание Ледяного Пламени\" закрыто",
            "2026-09-01 12:04:00.000 [Warning][Server] После события"
        };
        string report = Program.BuildReport(lines);
        StringAssertContains(report, "Предупреждений во время события: 0");
        StringAssertContains(report, "Ошибок во время события: 1");
    }

    [Test]
    public void EmptyInput_ReturnsError()
    {
        Assert.That(Program.BuildReport(Array.Empty<string>()), Does.StartWith("Ошибка:"));
    }

    private static void StringAssertContains(string actual, string expected) => Assert.That(actual, Does.Contain(expected));
}
