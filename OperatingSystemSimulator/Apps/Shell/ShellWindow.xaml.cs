using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;
using OperatingSystemSimulator.EventHandlers;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellWindow : UserControl
{
    public int EId { get; set; }
    public bool IsApp { get; set; } = true;

    private string? _title;
    private Popup? popupInstance;

    private Point initialPopupPosition;

    public string? title
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
        if (IsApp)
        {
            if (ProcessManager.Instance.GetProcessByPid(EId).App is NotepadApp notepadApp)
            {
                notepadApp.TryTerminate();
            }
            else
            {
                ProcessManager.Instance.TerminateProcess(EId, TerminateReasons.Self);
            }
        }
        else
        {
            MessageManager.Instance.GetMessageBlock(EId).HandleClosed();
            MessageManager.Instance.Close(EId);
        }
    }

    private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(Window.Current.Content as UIElement).Position;

        MouseEventsHandler.Instance.StartDragging(EId, pointerPosition, IsApp);
    }

    private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (IsApp)
        {
            ProcessManager.Instance.BringToFront(EId);
        }
        else
        {
            MessageManager.Instance.BringToFront(EId);
        }
    }
}
