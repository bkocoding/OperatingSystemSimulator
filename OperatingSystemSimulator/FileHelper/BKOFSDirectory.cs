namespace OperatingSystemSimulator.FileHelper;
public class BKOFSDirectory(string name, int dirID)
{
    public string Name { get; set; } = name;
    public int DirID { get; set; } = dirID;
    public bool IsRestricted { get; set; } = false;
    public int? ParentDirectoryID { get; set; }
    public List<BKOFSDirectory> ChildDirectories { get; set; } = [];
    public DateTime CreatedAt { get; } = DateTime.Now.ToLocalTime();
    public DateTime LastChanged { get; set; } = DateTime.Now.ToLocalTime();
    public List<BKOFSFile> Files { get; set; } = [];

    public string GetPath()
    {
        if (ParentDirectoryID == null)
        {
            return Name;
        }

        var parentDirectory = BKOFSManager.Instance.GetDirectoryById(ParentDirectoryID.Value);
        return $"{parentDirectory.GetPath()}/{Name}";
    }

    public int GetTotalSize()
    {
        int totalSize = Files.Sum(file => file.Size);
        foreach (var childDir in ChildDirectories)
        {
            totalSize += childDir.GetTotalSize();
        }
        return totalSize;
    }

}
