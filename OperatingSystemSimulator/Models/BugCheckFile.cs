using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Models;
public class BugCheckFile
{
    public string Reason { get; set; } = "";
    public string CausedBy { get; set; } = "";
    public string CreatedAt { get; } = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy");

    public List<ProcessBlock> MemoryDump { get; set; } = [];
}
