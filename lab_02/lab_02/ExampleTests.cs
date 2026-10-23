using NUnit.Framework;
using ServerConfigApp;
using System.Linq;

namespace ServerConfigTests;

[TestFixture]
public class ExampleTests
{
    private static ServerConfig ValidConfig() => new(
        MaxPlayers: 20, RamGb: 4, Port: 25565,
        OnlineMode: true, PvpEnabled: true, PvpAllowed: true,
        WhitelistEnabled: false, WhitelistPlayers: 0,
        IsPublic: true, DdosProtection: true);

    [Test]
    public void CorrectConfiguration_IsReady()
    {
        var issues = ConfigValidator.Check(ValidConfig());
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Ready));
    }

    [Test]
    public void DisabledAccountVerification_GivesWarning()
    {
        var issues = ConfigValidator.Check(ValidConfig() with { OnlineMode = false });
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Warning));
    }

    [Test]
    public void InvalidPort_BlocksStart()
    {
        var issues = ConfigValidator.Check(ValidConfig() with { Port = 70000 });
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Blocked));
    }

    [Test]
    public void ManyPlayersWithInsufficientRam_BlocksStart()
    {
        var issues = ConfigValidator.Check(ValidConfig() with { MaxPlayers = 60, RamGb = 2 });
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Blocked));
    }

    [Test]
    public void ConflictingPvpSettings_BlocksStart()
    {
        var issues = ConfigValidator.Check(ValidConfig() with { PvpAllowed = false });
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Blocked));
    }

    [Test]
    public void EmptyWhitelist_GivesWarning()
    {
        var issues = ConfigValidator.Check(ValidConfig() with { WhitelistEnabled = true });
        Assert.That(ConfigValidator.GetStatus(issues), Is.EqualTo(StartStatus.Warning));
    }

    [Test]
    public void MultipleProblems_AreAllReported()
    {
        var config = ValidConfig() with { Port = 70000, OnlineMode = false, DdosProtection = false };
        var issues = ConfigValidator.Check(config);
        Assert.That(issues.Count, Is.EqualTo(3));
        Assert.That(issues.Count(i => i.Severity == Severity.Error), Is.EqualTo(1));
        Assert.That(issues.Count(i => i.Severity == Severity.Warning), Is.EqualTo(2));
    }
}
