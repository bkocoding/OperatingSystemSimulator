using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class MemoryManager
{
    private static MemoryManager? instance;
    private static readonly object lockObject = new();
    private static readonly int pageSize = 4096;
    public static readonly int memorySize = 64000000;

    public ObservableCollection<MemoryBlock> MemoryBlocks { get; private set; }

    private MemoryManager()
    {
        MemoryBlocks = [];
    }

    public static MemoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance = new MemoryManager();
                }
            }
            return instance;
        }
    }

   public MemoryBlock? AllocateMemory(ProcessBlock processBlock, int size)
    {
        int startAddress = 0;
        int endAddress = 0;
        foreach (MemoryBlock block in MemoryBlocks)
        {
            if (block.ProcessBlock == null)
            {
                if (block.Size >= size)
                {
                    startAddress = block.StartAddress;
                    endAddress = block.StartAddress + size;
                    block.ProcessBlock = processBlock;
                    block.Size = block.Size - size;
//                    block.StartAddress = block.StartAddress + size;
                    return new MemoryBlock(processBlock, startAddress, endAddress);
                }
            }
        }
        return null;
    }

    public void DeallocateMemory(ProcessBlock processBlock)
    {
        foreach (MemoryBlock block in MemoryBlocks)
        {
            if (block.ProcessBlock == processBlock)
            {
                block.ProcessBlock = null;
                block.Size = block.EndAddress - block.StartAddress;
            }
        }
    }

    public void InitializeMemory()
    {
        MemoryBlocks.Add(new MemoryBlock(0, memorySize));
    }

}
