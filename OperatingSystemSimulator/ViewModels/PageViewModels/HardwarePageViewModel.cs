using System.ComponentModel;
using System.Runtime.CompilerServices;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.ViewModels.PageViewModels;

public class HardwarePageViewModel : INotifyPropertyChanged
{
    private static HardwarePageViewModel? instance;
    private static readonly object lockObject = new();

    public static HardwarePageViewModel Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance = new HardwarePageViewModel();
                }
            }
            return instance;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public HardwarePage? hardwarePage { get; set; }

    //private string _runningProcess = "";

    //private string RunningProcess
    //{
    //    get => _runningProcess;
    //    set
    //    {
    //        _runningProcess = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private string _hdOperation = "DISK NOT MOUNTED";

    //private string HdOperation
    //{
    //    get => _hdOperation;
    //    set
    //    {
    //        _hdOperation = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _keyStroke = HardwareStatuses.Idle.GetBrush();

    //private SolidColorBrush KeyStroke
    //{
    //    get => _keyStroke;
    //    set
    //    {
    //        _keyStroke = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _networkOutput = HardwareStatuses.Idle.GetBrush();
    //private SolidColorBrush NetworkOutput
    //{
    //    get => _networkOutput;
    //    set
    //    {
    //        _networkOutput = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _networkInput = HardwareStatuses.Idle.GetBrush();
    //private SolidColorBrush NetworkInput
    //{
    //    get => _networkInput;
    //    set
    //    {
    //        _networkInput = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _audioOutput = HardwareStatuses.Idle.GetBrush();
    //private SolidColorBrush AudioOutput
    //{
    //    get => _audioOutput;
    //    set
    //    {
    //        _audioOutput = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _hdWrite = HardwareStatuses.Idle.GetBrush();
    //private SolidColorBrush HdWrite
    //{
    //    get => _hdWrite;
    //    set
    //    {
    //        _hdWrite = value;
    //        OnPropertyChanged();
    //    }
    //}

    //private SolidColorBrush _hdRead = HardwareStatuses.Idle.GetBrush();
    //private SolidColorBrush HdRead
    //{
    //    get => _hdRead;
    //    set
    //    {
    //        _hdRead = value;
    //        OnPropertyChanged();
    //    }
    //}

    public void ShutDownStatusesChange()
    {
        //KeyStroke = HardwareStatuses.Idle.GetBrush();
        //NetworkOutput = HardwareStatuses.Idle.GetBrush();
        //NetworkInput = HardwareStatuses.Idle.GetBrush();
        //AudioOutput = HardwareStatuses.Idle.GetBrush();
        //HdWrite = HardwareStatuses.Idle.GetBrush();
        //HdRead = HardwareStatuses.Idle.GetBrush();
        //HdOperation = "DISK NOT MOUNTED";
        //RunningProcess = "";

        if (hardwarePage != null)
        {
            hardwarePage.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.AudioOutput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
            hardwarePage.SetHDOperation(HDOperations.NotMounted);
            hardwarePage.SetRunningProcess("");
        }
    }

    public void BugCheckStatusesChange() 
    {
        if (hardwarePage != null)
        {
            ProcessManager.Instance.IsTurnedOn = false;
            hardwarePage.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.AudioOutput, HardwareStatuses.Idle);
            hardwarePage.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Running);
            hardwarePage.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
            hardwarePage.SetHDOperation(HDOperations.OperatingSystem);
            hardwarePage.SetRunningProcess("Kernel");
        }
    }

    public void SetHardwareStatus(HardwareProperties property, HardwareStatuses status)
    {
        //var brush = status.GetBrush();

        //switch (property)
        //{
        //    case HardwareProperties.KeyStroke:
        //        KeyStroke = brush;
        //        break;

        //    case HardwareProperties.NetworkOutput:
        //        NetworkOutput = brush;
        //        break;

        //    case HardwareProperties.NetworkInput:
        //        NetworkInput = brush;
        //        break;

        //    case HardwareProperties.AudioOutput:
        //        AudioOutput = brush;
        //        break;

        //    case HardwareProperties.HdWrite:
        //        HdWrite = brush;
        //        break;

        //    case HardwareProperties.HdRead:
        //        HdRead = brush;
        //        break;

        //    default:
        //        throw new ArgumentOutOfRangeException(nameof(property), property, null);
        //}

        if(hardwarePage != null)
        {
            hardwarePage.SetHardwareStatus(property, status);
        }
    }

    public void SetHDOperation(HDOperations operation)
    {
        //HdOperation = operation;
        if (hardwarePage != null)
        {
            hardwarePage.SetHDOperation(operation);

        }
    }

    public void SetRunningProcess(string process)
    {
        //RunningProcess = process;
        if (hardwarePage != null)
        {
            hardwarePage.SetRunningProcess(process);
        }
    }

    public void ShowInfo(ProcessBlock processBlock) 
    {
        if (hardwarePage != null) 
        {
            hardwarePage.ShowInfo(processBlock);
        }
    }

    public void DismissInfo() 
    {
        if (hardwarePage != null)
        {
            hardwarePage.DismissInfo();
        }
    }

    public void ShowBiosInfo() 
    {
        if (hardwarePage != null)
        {
            hardwarePage.ShowBiosInfo();
        }
    }

}
