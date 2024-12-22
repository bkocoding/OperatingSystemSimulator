using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;
using OperatingSystemSimulator.EventHandlers;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellWindow : UserControl
{
    public int EId { get; set; }
    public ShellType CurrentShellType { get; set; } = ShellType.App;

    private string? _title;
    private Popup? popupInstance;

    private Point initialPopupPosition;

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
        if (CurrentShellType == ShellType.App)
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
        else if (CurrentShellType == ShellType.Message)
        {
            MessageManager.Instance.GetMessageBlock(EId).HandleCancel();
            MessageManager.Instance.Close(EId);
        }
        else if (CurrentShellType == ShellType.FileDialog) 
        {
            FileDialogManager.Instance.GetFileDialogBlock(EId).HandleCancel();
            FileDialogManager.Instance.Close(EId);
        }
    }

    private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(Window.Current.Content as UIElement).Position;

        MouseEventsHandler.Instance.StartDragging(EId, pointerPosition, CurrentShellType);
    }

    private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (CurrentShellType == ShellType.App)
        {
            ProcessManager.Instance.BringToFront(EId);
        }
        else if (CurrentShellType == ShellType.Message)
        {
            MessageManager.Instance.BringToFront(EId);
        }
        else if (CurrentShellType == ShellType.FileDialog) 
        {
            FileDialogManager.Instance.BringToFront(EId);
        }
    }
}
