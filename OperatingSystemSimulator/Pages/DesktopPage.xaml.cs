using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Apps.WebBrowser;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.NetworkHelper;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Pages;
public sealed partial class DesktopPage : Page
{
    public DesktopViewModel ViewModel { get; set; }
    public DesktopPage()
    {
        InitializeComponent();
        ViewModel = new DesktopViewModel();
        DataContext = ViewModel;
    }

    private void Shutdown_Click(object sender, RoutedEventArgs e)
    {
        ConsoleLogger.Log("Shutdown initiated...", LogType.Info);
        MessageManager.Instance.TerminateAllMessages();
        FileDialogManager.Instance.TerminateAllFileDialogs();
        ProcessManager.Instance.TerminateAllProcesses(TerminateReasons.System);
        Frame.Navigate(typeof(ShutdownPage));
    }
    private async void WifiButton_Click(object sender, RoutedEventArgs e)
    {
        var wifiIcon = (TextBlock)FindName("WifiIconTB");
        var wifiStatus = (TextBlock)FindName("WİfiStatusTB");

        if (!NetworkManager.Instance.IsConnected)
        {
            await NetworkManager.Instance.ConnectAsync();
            wifiIcon.Text = "🛜";
            wifiStatus.Text = "Connected";
        }
        else
        {
            NetworkManager.Instance.Disconnect();
            wifiIcon.Text = "🌐";
            wifiStatus.Text = "Disconnected";

        }
    }
    private async void APP1_Click(object sender, RoutedEventArgs e)
    {
        string title = "Test App";
        await ProcessManager.Instance.CreateProcess(new TestApp(), title, false, true);

    }

    private async void TaskMgr_Click(object sender, RoutedEventArgs e)
    {
        string title = "Task Manager";
        await ProcessManager.Instance.CreateProcess(new TaskManagerApp(), title, true, false);
    }

    private async void Notepad_Click(object sender, RoutedEventArgs e)
    {
        string title = "Notepad";
        await ProcessManager.Instance.CreateProcess(new NotepadApp(), title, false, false);
    }

    private async void FileExplorer_Click(object sender, RoutedEventArgs e)
    {
        string title = "File Explorer";
        await ProcessManager.Instance.CreateProcess(new FileExplorerApp(), title, false, false);
    }

    private async void WebBrowser_Click(object sender, RoutedEventArgs e)
    {
        string title = "Web Browser";
        await ProcessManager.Instance.CreateProcess(new WebBrowserApp(), title, false, false);
    }
}
