using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Extras.ConsoleLogger;
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
        ConsoleLogger.Log("Shutdown initiated...",LogType.Info);
        ProcessManager.Instance.TerminateAllProcesses(TerminateReasons.System);
        MessageManager.Instance.TerminateAllMessages();
        FileDialogManager.Instance.TerminateAllFileDialogs();
        Frame.Navigate(typeof(ShutdownPage));
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
}
