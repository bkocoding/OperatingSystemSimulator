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
        var testAppPopup = new Popup();
        string title = "Test APP";
        var testApp = new TestApp(title);
        var processBlock = ProcessManager.Instance.CreateProcess(testAppPopup, testApp, title);
        testApp.Pid = processBlock.Pid;

    }
}
