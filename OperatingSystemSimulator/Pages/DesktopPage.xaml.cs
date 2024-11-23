using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps;
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
        Frame.Navigate(typeof(ShutdownPage));
    }

    private void APP1_Click(object sender, RoutedEventArgs e)
    {
        string title = "Test APP";
        ProcessManager.Instance.CreateProcess(new Popup(), new TestApp(title), title, false);

    }

    private void TaskMgr_Click(object sender, RoutedEventArgs e)
    {
        string title = "Task Manager";
        ProcessManager.Instance.CreateProcess(new Popup(), new TaskManagerApp(title), title, true);
    }
}
