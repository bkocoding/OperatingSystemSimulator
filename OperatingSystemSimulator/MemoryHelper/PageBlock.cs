using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class PageBlock
{
    public int PageNumber { get; set; }
    public ProcessBlock? ProcessBlock { get; set; }
    public int UsedSpace { get; set; }
    public bool IsEmpty { get; set; }
    public bool IsAllocated { get; set; }
    public bool IsAdditional { get; set; }

}
