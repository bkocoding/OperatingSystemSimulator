using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class MemoryBlock
{
    public ProcessBlock ProcessBlock { get; }
    public int Size { get; private set; }
    public int StartAddress { get; private set; }
    public int EndAddress { get; private set; }

    public MemoryBlock(ProcessBlock processBlock, int startAddress, int endAddress)
    {
        ProcessBlock = processBlock;
        StartAddress = startAddress;
        EndAddress = endAddress;
        Size = EndAddress-StartAddress;
    }

}
