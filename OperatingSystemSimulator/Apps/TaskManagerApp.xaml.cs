using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.ViewModels.AppViewModels;

namespace OperatingSystemSimulator.Apps;
public sealed partial class TaskManagerApp : UserControl
{
    private int _pid;
    public TaskManagerViewModel ViewModel { get; set; }
    public int Pid
    {
        get => _pid;
        set
        {
            _pid = value;
            ShellTitleBar.Pid = _pid;
        }
    }
    public TaskManagerApp(string title)
    {
        InitializeComponent();
        ViewModel = new TaskManagerViewModel();
        DataContext = ViewModel;
        ShellTitleBar.Title = title;
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void BringToFront_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            int pid = (int)button.Tag;
            var process = ProcessManager.Instance.GetProcessByPid(pid);
            if (process != null && !process.IsRequired)
            {
                ProcessManager.Instance.BringToFront(pid);
            }
        }
    }
    private void Terminate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            int pid = (int)button.Tag;
            ProcessManager.Instance.TerminateProcess(pid, TerminateReasons.User);
        }
    }
    private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }
}
