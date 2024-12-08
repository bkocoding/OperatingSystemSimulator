using Microsoft.Maui;
using Windows.UI;

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
}
