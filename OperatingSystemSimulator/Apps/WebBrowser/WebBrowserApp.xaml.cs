using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace OperatingSystemSimulator.Apps.WebBrowser;
public sealed partial class WebBrowserApp : UserControl
{

    private int _pid;
    public int Pid
    {
        get => _pid;
        set
        {
            _pid = value;
            ShellTitleBar.EId = _pid;
            BrowserViewModel.PID = _pid;
        }
    }

    public BrowserViewModel BrowserViewModel;

    public WebBrowserApp()
    {
        InitializeComponent();
        BrowserViewModel = new BrowserViewModel(Pid);
        DataContext = BrowserViewModel;
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        BrowserViewModel.RefreshPage();

    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        BrowserViewModel.NavigateToHomePage();

    }

    private void GoButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);

        if (AddressBarTextBox.Text != string.Empty)
        {
            BrowserViewModel.NavigateTo(AddressBarTextBox.Text);
        }

    }

    private async void AddressBarTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        await ProcessManager.Instance.InterruptQueueAsync(Pid);
        if (e.Key == VirtualKey.Enter)
        {
            GoButton_Click(sender, e);
            AddressBarTextBox.IsEnabled = false;
            AddressBarTextBox.IsEnabled = true;
        }
    }
}
