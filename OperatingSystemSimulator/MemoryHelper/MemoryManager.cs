using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.MemoryHelper;
public class MemoryManager
{
    private static MemoryManager? instance;
    private static readonly object lockObject = new();
    //private static readonly int pageSize = 4096;
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
    //TODO: MemoryManager to be implemented
    //public bool IsMemoryHasAvailableSpace(int size)
    //{

    //    return true;
    //}

    public MemoryBlock AllocateMemory(ProcessBlock processBlock)
    {
        int startAdr = 0;
        int endAdr = 0;
        MemoryBlock memoryBlock = new(processBlock, startAdr, endAdr);
        MemoryBlocks.Add(memoryBlock);
        return memoryBlock;
    }

}
