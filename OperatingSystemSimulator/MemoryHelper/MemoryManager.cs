using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OperatingSystemSimulator.MemoryHelper;
public class MemoryManager : INotifyPropertyChanged
{
    private static MemoryManager? instance;
    private static readonly object lockObject = new();
    public static readonly int pageSize = 80000;
    public static readonly int memorySize = 64000000;
    private readonly System.Timers.Timer cleanupTimer;

    public ObservableCollection<PageBlock> Pages { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPagesChanged()
    {
        foreach (var page in Pages)
        {
            page.IsSelected = false;
        }
        HardwarePageViewModel.Instance.DismissInfo();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pages)));
    }

    private MemoryManager()
    {
        Pages = [];

        int totalPages = memorySize / pageSize;
        for (int i = 0; i < totalPages; i++)
        {
            Pages.Add(new PageBlock { PageNumber = i, IsEmpty = true, IsAllocated = false });
        }

        cleanupTimer = new System.Timers.Timer(12000);
        cleanupTimer.Elapsed += (sender, e) => CleanUnusedPages();
        cleanupTimer.Start();
    }

    public static MemoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new MemoryManager();
                }
            }
            return instance;
        }
    }

    public UtilizationResult AllocateMemory(ProcessBlock processBlock)
    {
        int requiredPages = (int)Math.Ceiling((double)processBlock.Size / pageSize);

        if (Pages.Count(p => p.IsEmpty) < requiredPages)
            return UtilizationResult.OutOfMemory;

        int allocatedPages = 0;
        foreach (var page in Pages.Where(p => p.IsEmpty).Take(requiredPages))
        {
            page.ProcessBlock = processBlock;
            page.IsEmpty = false;
            page.IsAllocated = true;
            page.IsAdditional = false;

            if (allocatedPages == requiredPages - 1)
            {
                int remainingSize = processBlock.Size - (allocatedPages * pageSize);
                page.UsedSpace = remainingSize;
                if (page.UsedSpace != pageSize)
                {
                    page.IsAdditional = true;
                }
            }
            else
            {
                page.UsedSpace = pageSize;
            }
            processBlock.PageBlocks.Add(page);
            allocatedPages++;
        }

        OnPagesChanged();
        return UtilizationResult.Success;
    }

    public UtilizationResult RequestAdditionalPages(ProcessBlock processBlock, int additionalPages)
    {

        if (Pages.Count(p => p.IsEmpty) < additionalPages)
            return UtilizationResult.OutOfMemory;

        foreach (var page in Pages.Where(p => !p.IsAllocated).Take(additionalPages))
        {
            page.ProcessBlock = processBlock;
            page.IsEmpty = true;
            page.IsAllocated = true;
            page.IsAdditional = true;
            processBlock.PageBlocks.Add(page);
        }

        OnPagesChanged();
        return UtilizationResult.Success;
    }

    public int WriteToAdditionalPages(int pid, int size)
    {
        int currentSize = size;

        var PagesList = Pages.Where(p => p.ProcessBlock != null && p.ProcessBlock.Pid == pid && p.IsAdditional);

        foreach (var page in PagesList)
        {
            if (page.UsedSpace == pageSize)
            {
                continue;
            }

            int freeSpace = pageSize - page.UsedSpace;
            if (currentSize <= freeSpace)
            {
                page.UsedSpace += currentSize;
                currentSize = 0;
                page.IsEmpty = false;
                break;
            }
            else
            {
                int otherFreeSpace = pageSize - page.UsedSpace;
                currentSize -= otherFreeSpace;
                page.UsedSpace = pageSize;
                page.IsEmpty = false;
                continue;
            }
        }


        if (currentSize > 0)
        {
            var result = RequestAdditionalPages(ProcessManager.Instance.GetProcessByPid(pid)!, (int)Math.Ceiling((double)currentSize / pageSize));
            if (result == UtilizationResult.OutOfMemory)
            {
                DeleteFromAdditionalPages(pid, size - currentSize);
                return currentSize;
            }
            currentSize = WriteToAdditionalPages(pid, currentSize);

        }

        OnPagesChanged();
        return 0;
    }

    public void DeleteFromAdditionalPages(int pid, int size)
    {
        int currentSize = size;
        foreach (var page in Pages.Where(p => p.ProcessBlock != null && p.ProcessBlock.Pid == pid && p.IsAdditional).Reverse())
        {
            if (page.IsEmpty)
            {
                continue;
            }

            if (currentSize <= page.UsedSpace)
            {
                page.UsedSpace -= currentSize;
                if (page.UsedSpace == 0)
                {
                    page.IsEmpty = true;
                }
                break;
            }
            else
            {
                currentSize -= page.UsedSpace;
                page.UsedSpace = 0;
                page.IsEmpty = true;
                continue;
            }
        }
    }

    public void DeallocateMemory(ProcessBlock processBlock)
    {
        foreach (var page in Pages.Where(p => p.ProcessBlock == processBlock))
        {
            page.ProcessBlock = null;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.IsAdditional = false;
            page.UsedSpace = 0;
            processBlock.PageBlocks.Clear();
        }
        OnPagesChanged();
    }

    public void DeallocateBios()
    {
        for (int i = 0; i <= 14; i++)
        {
            var page = Pages.Where(p => p.PageNumber == i).First();
            page.ProcessBlock = null;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.IsAdditional = false;
            page.UsedSpace = 0;
        }
        OnPagesChanged();
    }

    public List<ProcessBlock> GetAllProcesses()
    {
        return Pages.Where(p => p.ProcessBlock != null).Select(p => p.ProcessBlock!).Distinct().ToList();
    }

    public List<PageBlock> GetProcessPages(ProcessBlock processBlock)
    {
        return Pages.Where(p => p.ProcessBlock == processBlock).ToList();
    }

    public List<PageBlock> GetAdditionalProcessPages(ProcessBlock processBlock)
    {
        return Pages.Where(p => p.ProcessBlock == processBlock && p.IsAdditional).ToList();
    }

    public int GetTotalMemoryUsage(ProcessBlock processBlock)
    {
        int totalSize = 0;
        foreach (var page in Pages.Where(p => p.ProcessBlock == processBlock).ToList())
        {
            totalSize += page.UsedSpace;
        }
        return totalSize;
    }

    private bool CleanUnusedPages()
    {
        var unusedPages = Pages.Where(p => p.IsEmpty && p.IsAllocated && p.IsAdditional).ToList();
        foreach (var page in unusedPages)
        {
            page.ProcessBlock!.PageBlocks.Remove(page);
            page.ProcessBlock = null;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.IsAdditional = false;
        }


        if (unusedPages.Count > 0)
        {
            OnPagesChanged();
            ConsoleLogger.Log($"A memory cleaning event has been occured, {unusedPages.Count} page(s) have been freed.", LogType.Info);
        }

        return unusedPages.Count > 0;
    }

    public async Task TestMemory()
    {
        AllocateForBIOS();
        var oddPages = Pages.Where(p => p.PageNumber % 2 != 0 && p.PageNumber > 14).ToList();
        var evenPages = Pages.Where(p => p.PageNumber % 2 == 0 && p.PageNumber > 14).ToList();
        var processBlock = new ProcessBlock(0, "RAM TEST DATA", true);
        processBlock.Size = pageSize;
        ConsoleLogger.Log("Testing Ram...", LogType.Info);
        foreach (var page in oddPages)
        {
            page.UsedSpace = pageSize;
            page.IsEmpty = false;
            page.IsAllocated = true;
            page.ProcessBlock = processBlock;
            OnPagesChanged();
        }
        await Task.Delay(4000);

        foreach (var page in oddPages)
        {
            page.UsedSpace = 0;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.ProcessBlock = null;
            OnPagesChanged();
        }
        await Task.Delay(250);

        foreach (var page in evenPages)
        {
            page.UsedSpace = pageSize;
            page.IsEmpty = false;
            page.IsAllocated = true;
            page.ProcessBlock = processBlock;
            OnPagesChanged();
        }

        await Task.Delay(2000);

        foreach (var page in evenPages)
        {
            page.UsedSpace = 0;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.ProcessBlock = null;
            OnPagesChanged();
        }
    }

    public void AllocateForBIOS()
    {
        for (int i = 0; i <= 13; i++)
        {
            var page = Pages.Where(p => p.PageNumber == i).First();
            page.UsedSpace = pageSize;
            page.IsEmpty = false;
            page.IsAllocated = true;
            OnPagesChanged();
        }
        var lastPage = Pages.Where(p => p.PageNumber == 14).First();
        lastPage.UsedSpace = 68000;
        lastPage.IsEmpty = false;
        lastPage.IsAllocated = true;
    }

}
