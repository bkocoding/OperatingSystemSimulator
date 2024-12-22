using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OperatingSystemSimulator.ViewModels.PageViewModels;

public class DesktopViewModel : INotifyPropertyChanged
{
    private string? _dateTimeText;

    public string DateTimeText
    {
        get => _dateTimeText!;
        set
        {
            _dateTimeText = value;
            OnPropertyChanged();
        }
    }

    private Timer _timer;

    public DesktopViewModel()
    {
        UpdateDateTime();
        _timer = new Timer(UpdateDateTimeCallback!, null, 0, 1000);
    }

    private void UpdateDateTimeCallback(object state)
    {
        UpdateDateTime();
    }

    private void UpdateDateTime()
    {
        DateTimeText = DateTime.Now.ToString("HH:mm") + "\n" + DateTime.Now.ToString("dd.MM.yyyy");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
