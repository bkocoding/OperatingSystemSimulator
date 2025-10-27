using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Interfaces;
using OperatingSystemSimulator.Apps.Shell.Enums;

namespace OperatingSystemSimulator.Apps;
public sealed partial class TaskManagerApp : UserControl, IApp
{
    public TaskManagerViewModel ViewModel { get; set; }

    private int _pid;
    public int Pid
    {
        get => _pid;
        set
        {
            _pid = value;
            ShellTitleBar.EId = _pid;
        }
    }

    public AppType ApplicationType => AppType.TaskManager;

    public TaskManagerApp()
    {
        InitializeComponent();
        ViewModel = new TaskManagerViewModel();
        DataContext = ViewModel;
        ShellTitleBar.Title = "Task Manager";
        ShellTitleBar.CurrentAppType = ApplicationType;
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void BringToFront_Click(object sender, RoutedEventArgs e)
    {

        var button = sender as Button;
        if (button != null)
        {
            int pid = (int)button.Tag;
            if (!ProcessManager.Instance.GetProcessByPid(pid)!.IsRequired)
            {
                if (pid != Pid)
                {
                    ProcessManager.Instance.InterruptQueueAsync(Pid);
                }
                ProcessManager.Instance.BringToFront(pid);
            }
        }
    }

    private void Terminate_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.InterruptQueueAsync(Pid);
        var button = sender as Button;
        if (button != null)
        {
            int pid = (int)button.Tag;

            if (pid != Pid)
            {
                ProcessManager.Instance.InterruptQueueAsync(Pid);
            }
            else if (ProcessManager.Instance.GetProcessByPid(pid)!.App is NotepadApp notepad)
            {
                notepad.UnsubscribeToFocusedPopUpChangedEvent();
            }
            else if (ProcessManager.Instance.GetProcessByPid(pid)!.App is FileExplorerApp fileExplorer)
            {
                fileExplorer.UnsubscribeToFocusedPopUpChangedEvent();
            }

            ProcessManager.Instance.TerminateProcess(pid, TerminateReasons.User);
            //Weird bug with the gesture recogniser: fail: Microsoft.UI.Input.GestureRecognizer[0]
            //Microsoft.UI.Xaml.Controls.ScrollContentPresenter Inconsistent state, we already have a pending gesture for a pointer that is going down. Abort the previous gesture.
            //I added a filter to ignore this error in the app.xaml.cs...
            //It appears that the gesture recogniser is hanging but it does not affect the app for it has a ignore previous gesture function...
        }
    }

    private void Details_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            int pid = (int)button.Tag;
            var process = ProcessManager.Instance.GetProcessByPid(pid);
            if (process != null)
            {
                MessageManager.Instance.CreateMessage(Pid, process.Name, $"PID: {process.Pid}\nName: {process.Name}\nSize: {BKOFSManager.FormatSize(process.Size)}\nIdle Status: {(process.IsIdle ? "Idle" : "Not Idle")}", ShellType.App);
            }

        }
    }

    private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }
}
