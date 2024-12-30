using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using OperatingSystemSimulator.MemoryHelper;
using SkiaSharp;

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
        ViewModel.ShutDownStatusesChange();
        RunningProcess.Text = "BIOS Firmware";
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
        RamChart.Series = new List<ISeries>();
        RamChart.XAxes =
    [
        new Axis
        {
            MaxLimit = 68000000,
            IsVisible = false,
        }
    ];
        RamChart.YAxes =
        [
        new Axis
        {
            IsVisible = false,
        }
        ];
    }

    [Obsolete]
    private void UpdateRamChart()
    {
        var newSeries = new List<ISeries>();

        var processBlocks = MemoryManager.Instance.GetAllProcesses();
        int pageSize = MemoryManager.pageSize;

        int biosSize = 0;
        for (int i = 0; i <= 14; i++)
        {
            var page = MemoryManager.Instance.Pages.First(p => p.PageNumber == i);
            biosSize += page.UsedSpace;
        }

        newSeries.Add(new StackedRowSeries<int>
        {
            Values = new List<int> { biosSize },
            Stroke = new SolidColorPaint(SKColors.Black, 1),
            Fill = new SolidColorPaint(SKColors.Blue),
            //StackGroup = 0,
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            IsHoverable = false,
        });
        foreach (var processBlock in processBlocks)
        {
            var processPages = MemoryManager.Instance.GetProcessPages(processBlock);
            int processMemory = processPages.Sum(page => page.UsedSpace);


            var processSeries = new StackedRowSeries<int>
            {
                Values = new List<int> { processMemory },
                Stroke = new SolidColorPaint(SKColors.Black, 1),
                Fill = new SolidColorPaint(SKColors.Green),
                //StackGroup = 0,
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                IsHoverable = false,
                TooltipLabelFormatter = point =>
                {
                    return $"Process: {processBlock.Name}, Memory: {point.PrimaryValue}";
                }
            };


            newSeries.Add(processSeries);
        }

        int usedMemory = processBlocks.Sum(pb => MemoryManager.Instance.GetProcessPages(pb).Count * pageSize);
        int emptyMemory = MemoryManager.memorySize - usedMemory - biosSize;

        newSeries.Add(new StackedRowSeries<int>
        {
            Values = new List<int> { emptyMemory },
            Stroke = new SolidColorPaint(SKColors.Black, 1),
            Fill = new SolidColorPaint(SKColors.Gray),
            //StackGroup = 0,
            AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            IsHoverable = false
        });

        RamChart.Series = newSeries;
    }
}
