
using OperatingSystemSimulator.ViewModels.AppViewModels;

namespace OperatingSystemSimulator.Apps.WebBrowser.Pages;
public sealed partial class HomePage : UserControl
{
    private BrowserViewModel _browserViewModel;

    public HomePage(BrowserViewModel browserViewModel)
    {
        InitializeComponent();
        _browserViewModel = browserViewModel;
        
    }

    private void HorizonMusicButton_Click(object sender, RoutedEventArgs e)
    {
        _browserViewModel.NavigateTo("https://www.horizonmusic.com");
    }
}
