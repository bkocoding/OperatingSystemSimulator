using System.Collections.ObjectModel;
using System.ComponentModel;
using OperatingSystemSimulator.ProcessHelper;

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

        cleanupTimer = new System.Timers.Timer(120000);
        cleanupTimer.Elapsed += (sender, e) => CleanupUnusedPages();
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

            allocatedPages++;
        }

        OnPagesChanged();
        return UtilizationResult.Success;
    }

    public bool RequestAdditionalPages(ProcessBlock processBlock, int additionalPages)
    {

        if (Pages.Count(p => p.IsEmpty) < additionalPages)
            return false;

        foreach (var page in Pages.Where(p => p.IsAllocated).Take(additionalPages))
        {
            page.ProcessBlock = processBlock;
            page.IsEmpty = true;
            page.IsAllocated = true;
            page.IsAdditional = true;
        }

        OnPagesChanged();
        return true;
    }

    public void WriteToAdditionalPages(int Pid)
    {

    }

    public void DeallocateMemory(ProcessBlock processBlock)
    {
        foreach (var page in Pages.Where(p => p.ProcessBlock == processBlock))
        {
            page.ProcessBlock = null;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.IsAdditional = false;
        }
        OnPagesChanged();
    }

    public List<PageBlock> GetProcessPages(ProcessBlock processBlock)
    {
        return Pages.Where(p => p.ProcessBlock == processBlock).ToList();
    }

    public List<PageBlock> GetAdditionalProcessPages(ProcessBlock processBlock)
    {
        return Pages.Where(p => p.ProcessBlock == processBlock && p.IsAdditional).ToList();
    }

    private void CleanupUnusedPages()
    {
        if (Pages.Count(p => !p.IsEmpty) > (int)(Pages.Count * 0.7))
        {
            if (!TryCleanup())
            {
                return;
            }
        }

        TryCleanup();
    }

    private bool TryCleanup()
    {
        var unusedPages = Pages.Where(p => !p.IsEmpty && !p.IsAllocated).ToList();
        foreach (var page in unusedPages)
        {
            page.ProcessBlock = null;
            page.IsEmpty = true;
            page.IsAllocated = false;
            page.IsAdditional = false;
        }

        OnPagesChanged();
        return Pages.Count(p => !p.IsEmpty) <= (int)(Pages.Count * 0.7);
    }


}
