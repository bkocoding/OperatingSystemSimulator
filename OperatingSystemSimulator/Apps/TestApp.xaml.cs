using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps;
public sealed partial class TestApp : UserControl
{
    private int _pid;
    public int Pid
    {
        get => _pid;
        set
        {
            _pid = value;
            ShellTitleBar.EId = _pid;
            ContentText.Text = $"PID: {_pid}";
        }
    }

    public TestApp()
    {
        InitializeComponent();
        ShellTitleBar.title = "Test App";
        
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

}
