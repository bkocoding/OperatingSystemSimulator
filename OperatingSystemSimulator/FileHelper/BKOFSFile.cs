
namespace OperatingSystemSimulator.FileHelper;
public class BKOFSFile
{
    public string Name { get; set; }
    public BKOFSDirectory ParentDirectory { get; set; }
    public int FileID { get; }
    public string? Content { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool IsBusy { get; set; }
    public bool IsRestricted = false;
    public DateTime CreatedAt { get; } = DateTime.Now.ToLocalTime();
    public DateTime LastChanged { get; set; } = DateTime.Now.ToLocalTime();    

    public BKOFSFile(string name, BKOFSDirectory parentDir, int fileID, bool? isRestricted = null)
    {
        Name = name;
        ParentDirectory = parentDir;
        FileID = fileID;
        if (isRestricted != null)
        {
            IsRestricted = (bool)isRestricted;
        }
    }
}
