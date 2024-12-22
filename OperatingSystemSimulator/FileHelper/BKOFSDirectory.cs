namespace OperatingSystemSimulator.FileHelper;
public class BKOFSDirectory(string name, int dirID)
{
    public string Name { get; set; } = name;
    public int DirID { get; set; } = dirID;
    public bool IsRestricted { get; set; } = false;
    public BKOFSDirectory? ParentDirectory { get; set; }
    public List<BKOFSDirectory> ChildDirectories { get; set; } = [];
    public DateTime CreatedAt { get; } = DateTime.Now.ToLocalTime();
    public DateTime LastChanged { get; set; } = DateTime.Now.ToLocalTime();
    public List<BKOFSFile> Files { get; set; } = [];
}
