using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.ViewModels.AppViewModels;

namespace OperatingSystemSimulator.Apps;
public sealed partial class TaskManagerApp : UserControl
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
    public TaskManagerApp()
    {
        InitializeComponent();
        ViewModel = new TaskManagerViewModel();
        DataContext = ViewModel;
        ShellTitleBar.Title = "Task Manager";
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
    private void ProcessListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }
}
