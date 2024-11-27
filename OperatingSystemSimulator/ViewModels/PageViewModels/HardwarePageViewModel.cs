using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

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

    private string _runningProcess = "";

    public string RunningProcess
    {
        get => _runningProcess;
        set
        {
            _runningProcess = value;
            OnPropertyChanged();
        }
    }

    private string _hdOperation = "DISK NOT MOUNTED";

    public string HdOperation 
    {
        get => _hdOperation;
        set
        {
            _hdOperation = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _keyStrokeInput = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;

    public SolidColorBrush KeyStrokeInput
    {
        get => _keyStrokeInput;
        set
        {
            _keyStrokeInput = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _networkOutput = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;
    public SolidColorBrush NetworkOutput 
    {
        get => _networkOutput;
        set 
        {
            _networkOutput = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _networkInput = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;
    public SolidColorBrush NetworkInput
    {
        get => _networkInput;
        set
        {
            _networkInput = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _audioOutput = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;
    public SolidColorBrush AudioOutput
    {
        get => _audioOutput;
        set
        {
            _audioOutput = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _hdWrite = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;
    public SolidColorBrush HdWrite
    {
        get => _hdWrite;
        set
        {
            _hdWrite = value;
            OnPropertyChanged();
        }
    }

    private SolidColorBrush _hdRead = new BrushConverter().ConvertFromString("#FFFFFF") as SolidColorBrush;
    public SolidColorBrush HdRead
    {
        get => _hdRead;
        set
        {
            _hdRead = value;
            OnPropertyChanged();
        }
    }

}
