using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView;
using OperatingSystemSimulator.MemoryHelper;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Hardware;

public sealed partial class HardwarePage : Page
{
    private readonly HardwarePageViewModel ViewModel;
    public HardwarePage()
    {
        ViewModel = HardwarePageViewModel.Instance;
        //DataContext = ViewModel;
        ViewModel.hardwarePage = this;
        InitializeComponent();
        ViewModel.ResetStatuses();
        InitializeRamChart();
        UpdateRamChart();

        MemoryManager.Instance.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(MemoryManager.Pages))
            {
                UpdateRamChart();
            }
        };

    }

    public void SetRunningProcess(string processName)
    {
        RunningProcess.Text = processName;
    }

    public void SetHDOperation(HDOperations operation)
    {
        HdOperation.Text = operation.GetDescription();
    }

    public void SetHardwareStatus(HardwareProperties property, HardwareStatuses status)
    {
        switch (property)
        {
            case HardwareProperties.KeyStroke:
                KeyStrokeInput.Foreground = status.GetBrush();
                break;
            case HardwareProperties.HdWrite:
                HdWrite.Foreground = status.GetBrush();
                break;
            case HardwareProperties.HdRead:
                HdRead.Foreground = status.GetBrush();
                break;
            case HardwareProperties.AudioOutput:
                AudioOutput.Foreground = status.GetBrush();
                break;
            case HardwareProperties.NetworkInput:
                NetworkInput.Foreground = status.GetBrush();
                break;
            case HardwareProperties.NetworkOutput:
                NetworkOutput.Foreground = status.GetBrush();
                break;
        }
    }

    private void InitializeRamChart()
    {
        var processBlocks = ProcessManager.Instance.ProcessBlocks;
        int memorySize = MemoryManager.pageSize;
        int pageSize = MemoryManager.pageSize;

        var stackedRowSeries = new StackedRowSeries<int>
        {
            Values = new ObservableCollection<int>(),
        };

        foreach (var processBlock in processBlocks)
        {
            var processPages = MemoryManager.Instance.GetProcessPages(processBlock);
            int processMemory = 0;
            foreach (var page in processPages)
            {
                processMemory += page.UsedSpace;
            }
            stackedRowSeries.Values.Append(processMemory);
        }


        int usedMemory = processBlocks.Sum(pb => MemoryManager.Instance.GetProcessPages(pb).Count * pageSize);
        int emptyMemory = memorySize - usedMemory;
        stackedRowSeries.Values.Append(emptyMemory);

        RamChart.Series = [stackedRowSeries];
    }

    private void UpdateRamChart()
    {
        var processBlocks = ProcessManager.Instance.ProcessBlocks;
        int pageSize = MemoryManager.pageSize;

        if (RamChart.Series.Count() != 0)
        {
            var oldbarSeries = (ObservableCollection<int>)RamChart.Series.First().Values!;
            oldbarSeries.Clear();
        }

        var barSeries = (ObservableCollection<int>)RamChart.Series.First().Values!;
        foreach (var processBlock in processBlocks)
        {
            var processPages = MemoryManager.Instance.GetProcessPages(processBlock);
            int processMemory = 0;
            foreach (var page in processPages)
            {
                processMemory += page.UsedSpace;
            }
            barSeries.Add(processMemory);
        }

        int usedMemory = processBlocks.Sum(pb => MemoryManager.Instance.GetProcessPages(pb).Count * pageSize);
        int emptyMemory = MemoryManager.memorySize - usedMemory;
        barSeries.Add(emptyMemory);
    }
}
