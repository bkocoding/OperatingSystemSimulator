using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.EventHandlers;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellWindow : UserControl
{
    public int Pid { get; set; }

    private string? _title;

    public string? Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                titleText.Text = _title;
            }
        }
    }

    public ShellWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
    }

    private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(Window.Current?.Content as UIElement).Position;

        MouseEventsHandler.Instance.StartDragging(Pid, pointerPosition);
    }

    private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }
}
