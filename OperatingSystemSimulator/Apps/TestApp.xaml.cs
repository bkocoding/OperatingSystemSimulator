using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Interfaces;

namespace OperatingSystemSimulator.Apps;
public sealed partial class TestApp : UserControl, IApp
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

    public AppType ApplicationType => AppType.TestApp;

    public TestApp()
    {
        InitializeComponent();
        ShellTitleBar.CurrentAppType = ApplicationType;
        ShellTitleBar.Title = "Test App";

    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

}
