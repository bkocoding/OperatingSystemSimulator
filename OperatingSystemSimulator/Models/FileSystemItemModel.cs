namespace OperatingSystemSimulator.Models;

public class FileSystemItemModel
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime LastChanged { get; set; }
    public int Size { get; set; }
    public int Id { get; set; }
    public required object Content { get; set; }
}
