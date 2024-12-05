using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class MemoryBlock
{
    public ProcessBlock? ProcessBlock { get; set; }
    public int Size { get; set; }
    public int StartAddress { get; private set; }
    public int EndAddress { get; private set; }

    /// <summary>
    /// Creates a new MemoryBlock with a ProcessBlock. This is used when allocating memory.
    /// </summary>
    /// <param name="processBlock"></param>
    /// <param name="startAddress"></param>
    /// <param name="endAddress"></param>
    public MemoryBlock(ProcessBlock processBlock, int startAddress, int endAddress)
    {
        ProcessBlock = processBlock;
        StartAddress = startAddress;
        EndAddress = endAddress;
        Size = EndAddress-StartAddress;
    }
    /// <summary>
    /// Creates a new MemoryBlock without a ProcessBlock. This is used when Initializing memory.
    /// </summary>
    /// <param name="startAddress"></param>
    /// <param name="endAddress"></param>
    public MemoryBlock( int startAddress, int endAddress)
    {
        StartAddress = startAddress;
        EndAddress = endAddress;
        Size = EndAddress - StartAddress;
    }

}
