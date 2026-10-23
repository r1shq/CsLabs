using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

[TestFixture]
public class ExampleTests
{
    string root = null!;
    string saves = null!;
    string backups = null!;
    string index = null!;
    SaveEntry entry = null!;

    [SetUp]
    public void SetUp()
    {
        root = Path.Combine(Path.GetTempPath(), "save-manager-test-" + Guid.NewGuid());
        saves = Path.Combine(root, "saves");
        backups = Path.Combine(root, "backups");
        index = Path.Combine(root, "index.json");
        Directory.CreateDirectory(saves);
        entry = new SaveEntry { Name = "Test", File = "save.json", UpdatedAt = "2026-09-01T10:00:00" };
    }

    [TearDown]
    public void TearDown() { if (Directory.Exists(root)) Directory.Delete(root, true); }

    [Test]
    public void MissingIndex_ReturnsEmptyList() => Assert.That(Program.LoadIndex(index), Is.Empty);

    [Test]
    public void Index_RoundTrip()
    {
        Program.SaveIndex(index, new List<SaveEntry> { entry });
        var list = Program.LoadIndex(index);
        Assert.That(list, Has.Count.EqualTo(1));
        Assert.That(list[0].Name, Is.EqualTo("Test"));
    }

    [Test]
    public void InvalidIndex_ReturnsEmptyList()
    {
        File.WriteAllText(index, "{invalid json");
        Assert.That(Program.LoadIndex(index), Is.Empty);
    }

    [Test]
    public void Save_RoundTrip()
    {
        Program.SaveGame(saves, entry, new SaveData { Character="Герой", Map="Лес", Health=80, Position=new Position { X=3, Y=4 }, Inventory=new(){"лук"} });
        var data = Program.LoadSave(saves, entry);
        Assert.That(data, Is.Not.Null);
        Assert.That(data!.Position.Y, Is.EqualTo(4));
        Assert.That(data.Inventory, Does.Contain("лук"));
    }

    [Test]
    public void MissingSave_ReturnsNull() => Assert.That(Program.LoadSave(saves, entry), Is.Null);

    [Test]
    public void FindName_CaseInsensitive()
    {
        Assert.That(Program.FindByName(new List<SaveEntry> { entry }, "test"), Is.SameAs(entry));
    }

    [Test]
    public void Backup_AndRestore()
    {
        Program.SaveGame(saves, entry, new SaveData { Health=20 });
        Program.CreateBackup(saves, backups, entry);
        Program.SaveGame(saves, entry, new SaveData { Health=5 });
        Program.RestoreBackup(saves, backups, entry);
        Assert.That(Program.LoadSave(saves, entry)!.Health, Is.EqualTo(20));
    }

    [Test]
    public void MissingBackup_LeavesSaveUntouched()
    {
        Program.SaveGame(saves, entry, new SaveData { Health=20 });
        Program.RestoreBackup(saves, backups, entry);
        Assert.That(Program.LoadSave(saves, entry)!.Health, Is.EqualTo(20));
    }

    [Test]
    public void Delete_RemovesFileAndIndex()
    {
        var entries = new List<SaveEntry> { entry };
        Program.SaveGame(saves, entry, new SaveData());
        Program.DeleteSave(saves, index, entries, entry);
        Assert.That(File.Exists(Path.Combine(saves, "save.json")), Is.False);
        Assert.That(Program.LoadIndex(index), Is.Empty);
    }
}
