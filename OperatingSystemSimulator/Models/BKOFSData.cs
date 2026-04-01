namespace OperatingSystemSimulator.Models;
public class BKOFSData
{
    public required BKOFSDirectory RootDirectory { get; set; }
    public required int CurrentDirID { get; set; }
    public required int CurrentFileID { get; set; }
}
