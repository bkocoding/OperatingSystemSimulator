using Microsoft.UI.Xaml.Input;
using Newtonsoft.Json.Linq;
using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Shell.Enums;
using OperatingSystemSimulator.ToolTipHelper;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellWindow : UserControl
{
    public int EId { get; set; }

    private ShellType _currentShellType;
    public ShellType CurrentShellType
    {
        get => _currentShellType;
        set
        {
            _currentShellType = value;
            _tooltipManager.ApplyTooltip(Accessibility_button, new ToolTipHelper.ToolTipTools.ToolTipParameters { SType = CurrentShellType });
        }
    }

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

    TooltipManager _tooltipManager;

    private AppType _currentAppType;
    public AppType CurrentAppType
    {
        get => _currentAppType; set
        {
            _currentAppType = value;
            _tooltipManager.ApplyTooltip(Accessibility_button, new ToolTipHelper.ToolTipTools.ToolTipParameters { SType = CurrentShellType, AType = value });
        }
    }


    public ShellWindow()
    {
        InitializeComponent();
        _tooltipManager = new();


    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentShellType == ShellType.App)
        {
            if (ProcessManager.Instance.GetProcessByPid(EId)!.App is NotepadApp notepadApp)
            {
                notepadApp.TryTerminate();
            }
            else
            {
                await ProcessManager.Instance.TerminateProcess(EId, TerminateReasons.Self);
            }
        }
        else if (CurrentShellType == ShellType.Message)
        {
            MessageManager.Instance.GetMessageBlock(EId)!.HandleCancel();
            MessageManager.Instance.Close(EId);
        }
        else if (CurrentShellType == ShellType.FileDialog)
        {
            FileDialogManager.Instance.GetFileDialogBlock(EId)!.HandleCancel();
            FileDialogManager.Instance.Close(EId);
        }
    }

    private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(Window.Current!.Content as UIElement).Position;

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
