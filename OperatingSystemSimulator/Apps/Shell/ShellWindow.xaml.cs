using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Shell.Enums;
using OperatingSystemSimulator.ToolTipHelper;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellWindow : UserControl
{
    private int _eId;
    public int EId
    {
        get => _eId; set
        {
            _eId = value;
            UpdateTooltip();
        }
    }

    private ShellType _currentShellType;
    public ShellType CurrentShellType
    {
        get => _currentShellType;
        set
        {
            _currentShellType = value;
            UpdateTooltip();
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


    private AppType _currentAppType;
    public AppType CurrentAppType
    {
        get => _currentAppType; set
        {
            _currentAppType = value;
            UpdateTooltip();
        }
    }

    private FileDialogBlock? _currentFileDialogBlock;
    public FileDialogBlock? CurrentFileDialogBlock { get => _currentFileDialogBlock; set { _currentFileDialogBlock = value; UpdateTooltip(); } }

    private MessageBlock? _currentMessageBlock;
    public MessageBlock? CurrentMessageBlock { get => _currentMessageBlock; set { _currentMessageBlock = value; UpdateTooltip(); } }

    private readonly TooltipManager _tooltipManager;

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

    private void UpdateTooltip()
    {
        if (_tooltipManager == null || Accessibility_button == null)
            return;

        Dictionary<string, string> extraParams = new() { { "PID", $"{_eId}" } };

        if (CurrentShellType == ShellType.FileDialog)
        {
            if (CurrentFileDialogBlock != null)
            {
                extraParams.Clear();
                extraParams.Add("PID", $"{CurrentFileDialogBlock.BPId}");
                extraParams.Add("DID", $"{CurrentFileDialogBlock.DId}");
            }
        }
        else if (CurrentShellType == ShellType.Message)
        {
            if (CurrentMessageBlock != null)
            {
                extraParams.Clear();
                var process = ProcessManager.Instance.GetProcessByPid(CurrentMessageBlock.BSId);
                if (process != null)
                {
                    extraParams.Add("MessageBoxSenderApp", $"{process.ApplicationType}");
                }
                extraParams.Add("PID", $"{CurrentMessageBlock.BSId}");
                extraParams.Add("MID", $"{CurrentMessageBlock.MId}");
            }
        }

        _tooltipManager.ApplyTooltip(Accessibility_button, new ToolTipHelper.ToolTipTools.ToolTipParameters
        {
            SType = _currentShellType,
            AType = _currentAppType,
            ExtraParams = extraParams
        });
    }

}
