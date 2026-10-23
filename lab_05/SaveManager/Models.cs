using Newtonsoft.Json;

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class SaveData
{
    public string Character { get; set; } = "";
    public string Map { get; set; } = "";
    public Position Position { get; set; } = new();
    public int Health { get; set; }
    public List<string> Inventory { get; set; } = new();
}

public class SaveEntry
{
    public string Name { get; set; } = "";
    public string File { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}
