using System.ComponentModel;
using System.Text.RegularExpressions;
using OperatingSystemSimulator.Apps.WebBrowser;
using OperatingSystemSimulator.Apps.WebBrowser.Pages;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.MemoryHelper;
using OperatingSystemSimulator.NetworkHelper;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.ViewModels.AppViewModels;
public class BrowserViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private UserControl _currentPage = new HomePage();
    public UserControl CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage = value;
            OnPropertyChanged(nameof(CurrentPage));
        }
    }

    private string _currentTitle = "";
    public string CurrentTitle
    {
        get => _currentTitle;
        private set
        {
            if (_currentTitle != value)
            {
                _currentTitle = value;
                OnPropertyChanged(nameof(CurrentTitle));
            }
        }
    }

    private string _currentAddress = "";
    public string CurrentAddress
    {
        get => _currentAddress;
        private set
        {
            _currentAddress = value;
            OnPropertyChanged(nameof(CurrentAddress));
        }
    }

    private bool _isBackButtonEnabled = false;
    public bool IsBackButtonEnabled
    {
        get => _isBackButtonEnabled;
        private set
        {
            _isBackButtonEnabled = value;
            OnPropertyChanged(nameof(IsBackButtonEnabled));
        }
    }

    private bool _isForwardButtonEnabled = false;
    public bool IsForwardButtonEnabled
    {
        get => _isForwardButtonEnabled;
        private set
        {
            _isForwardButtonEnabled = value;
            OnPropertyChanged(nameof(IsForwardButtonEnabled));
        }
    }

    public int PID { get; set; }

    public BrowserViewModel(int pID)
    {
        CurrentPage = new HomePage();
        SetTitle("Home");
        SetAddress("browser://home");
        PID = pID;

    }

    public void SetTitle(string newTitle)
    {
        CurrentTitle = newTitle + " - Web Browser";
    }

    public void SetAddress(string newAddress)
    {
        CurrentAddress = newAddress;
    }


    public void RefreshPage()
    {
        NavigateTo(CurrentAddress);
    }

    public async void NavigateTo(string address)
    {
        string formattedAdress = FormatUrl(address);

        if (CurrentPage is HorizonMusicPage) 
        {
            MemoryManager.Instance.DeleteFromAdditionalPages(PID, 143049);
        }

        if (NetworkManager.Instance.IsConnected)
        {
            switch (formattedAdress)
            {
                case "browser://home":
                    NavigateToHomePage();
                    break;

                case "https://www.horizonmusic.com":
                    await NetworkManager.DNSQueryAsync();
                    ConsoleLogger.Log("DNS Query completed, IP address has been found.", LogType.NetworkInput);
                    await Load();

                    var result = MemoryManager.Instance.WriteToAdditionalPages(PID, 143049);
                    if (result > 0) 
                    {
                        NavigateToErrorPage(ErrorCodes.ERR_OUT_OF_MEMORY, formattedAdress);
                    }

                    await NetworkManager.DownloadingWebSiteAsync(formattedAdress);
                    NavigateToHorizonMusicPage();
                    break;

                case "browser://error":
                    NavigateToErrorPage(ErrorCodes.ERR_NO_ERROR, formattedAdress);
                    break;

                default:
                    await Load();
                    await NetworkManager.DNSQueryAsync();
                    ConsoleLogger.Log("DNS Query failed, IP address has been not found.", LogType.NetworkInput);
                    NavigateToErrorPage(ErrorCodes.ERR_NAME_NOT_RESOLVED, formattedAdress);
                    break;
            }
        }
        else
        {
            switch (formattedAdress)
            {
                case "browser://home":
                    NavigateToHomePage();
                    break;

                case "browser://error":
                    NavigateToErrorPage(ErrorCodes.ERR_NO_ERROR, formattedAdress);
                    break;

                default:
                    NavigateToErrorPage(ErrorCodes.ERR_INTERNET_DISCONNECTED, formattedAdress);
                    break;
            }
        }
    }

    public async Task Load()
    {
        await Task.Delay(200);
        CurrentPage = new LoadingPage();
        await Task.Delay(2000);

    }

    public void NavigateToHomePage()
    {
        if(CurrentPage is HorizonMusicPage horizonMusicPage)
        {
            MemoryManager.Instance.DeleteFromAdditionalPages(PID, 143049);
        }

        CurrentPage = new HomePage();
        SetTitle("Home");
        SetAddress("browser://home");

    }

    private void NavigateToErrorPage(ErrorCodes code, string errorAddress)
    {
        CurrentPage = new ErrorPage(code, errorAddress);
        SetTitle("Error");
        SetAddress(errorAddress);
    }

    private void NavigateToHorizonMusicPage()
    {
        CurrentPage = new HorizonMusicPage(this);
        SetTitle("Horizon Music");
        SetAddress("https://www.horizonmusic.com");
    }

    private static string FormatUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "";
        }

        if (url.StartsWith("browser://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        if (IsIpAddress(url))
        {
            return url;
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            return $"https://{url.Trim()}";
        }

        if (IsDomain(url))
        {
            return $"https://www.{url.Trim()}";
        }

        return url;
    }

    private static bool IsIpAddress(string input)
    {
        if (Regex.IsMatch(input, @"^(\d{1,3}\.){3}\d{1,3}$"))
        {
            return true;
        }

        if (Regex.IsMatch(input, @"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$"))
        {
            return true;
        }

        return false;
    }

    private static bool IsDomain(string input)
    {
        return Regex.IsMatch(input, @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void TryDispose()
    {
        if (CurrentPage is HorizonMusicPage horizonMusicPage)
        {
            horizonMusicPage.TryDispose();
        }
    }
}
