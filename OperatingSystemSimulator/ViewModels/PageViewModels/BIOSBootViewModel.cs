using System.ComponentModel;
using System.Runtime.CompilerServices;
using OperatingSystemSimulator.Services;

namespace OperatingSystemSimulator.ViewModels.PageViewModels;

public class BIOSBootViewModel : INotifyPropertyChanged
{
    private readonly BIOSSettingsService _biosSettingsService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BIOSBootViewModel()
    {
        _biosSettingsService = (Application.Current as App)?.Host?.Services.GetRequiredService<BIOSSettingsService>()
                               ?? throw new InvalidOperationException("BIOSSettingsService is not available");

        LoadSettings();
    }

    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private string fbo = string.Empty;
    public string FBO
    {
        get { return fbo; }
        set
        {
            fbo = value;
            OnPropertyChanged();
        }
    }

    private string sbo = string.Empty;
    public string SBO
    {
        get { return sbo; }
        set
        {
            sbo = value;
            OnPropertyChanged();
        }
    }

    private void LoadSettings()
    {
        FBO = _biosSettingsService.Settings.FirstBootOption;
        SBO = _biosSettingsService.Settings.SecondBootOption;
    }
}
