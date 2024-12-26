using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;
using Windows.UI.Core;

namespace OperatingSystemSimulator.EventHandlers;

public class MouseEventsHandler
{
    private static MouseEventsHandler? instance;
    private static readonly object lockObject = new();

    private Popup? draggingPopup;
    private ProcessBlock? draggingProcessBlock;
    private Point initialPointerOffset;

    public event Action<Point>? MouseMoved;

    public static MouseEventsHandler Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new MouseEventsHandler();
                    }
                }
            }
            return instance;
        }
    }

    private MouseEventsHandler()
    {
        Window.Current!.CoreWindow!.PointerMoved += OnPointerMoved;
        Window.Current!.CoreWindow!.PointerReleased += OnPointerReleased;
        Window.Current!.CoreWindow!.PointerPressed += OnPointerPressed;
    }

    private async void OnPointerPressed(CoreWindow sender, PointerEventArgs args)
    {
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Running);
        ProcessManager.Instance.InterruptQueueAsync(1);
        await Task.Delay(10);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);

    }

    public void Initialize()
    {
        return;
    }

    private async void OnPointerMoved(CoreWindow sender, PointerEventArgs e)
    {
        if (!ProcessManager.Instance.IsTurnedOn)
        {
            return;
        }
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Running);
        var pointerPosition = e.CurrentPoint.Position;
        var windowBounds = Window.Current!.Bounds;

        var constrainedX = Math.Max(0, Math.Min(pointerPosition.X, windowBounds.Width));
        var constrainedY = Math.Max(0, Math.Min(pointerPosition.Y, windowBounds.Height));

        if (draggingPopup != null && draggingProcessBlock != null)
        {
            var content = draggingPopup.Child as FrameworkElement;
            if (content != null)
            {
                var popupWidth = content.ActualWidth > 0 ? content.ActualWidth : 100;
                var popupHeight = content.ActualHeight > 0 ? content.ActualHeight : 100;

                // Desktop Page taskbar stackpanel check
                var maxHeight = windowBounds.Height - 55;

                var newLeft = Math.Max(0, Math.Min(constrainedX - initialPointerOffset.X, windowBounds.Width - popupWidth));
                var newTop = Math.Max(0, Math.Min(constrainedY - initialPointerOffset.Y, maxHeight - popupHeight));

                ProcessManager.Instance.InterruptQueueAsync(draggingProcessBlock.Pid);

                draggingPopup.HorizontalOffset = newLeft;
                draggingPopup.VerticalOffset = newTop;
            }
        }
        await Task.Delay(1);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);

    }

    private void OnPointerReleased(CoreWindow sender, PointerEventArgs e)
    {
        draggingPopup = null;
        draggingProcessBlock = null;
        //ProcessManager.Instance.InterruptQueueAsync(1);
    }

    public void StartDragging(int eid, Point initialPointerPosition, ShellType shellType)
    {

        if (shellType == ShellType.App)
        {
            draggingProcessBlock = ProcessManager.Instance.GetProcessByPid(eid);
            if (draggingProcessBlock == null)
            {
                return;
            }
            //ProcessManager.Instance.InterruptQueueAsync(draggingProcessBlock.Pid);
            draggingPopup = draggingProcessBlock.Popup;
        }
        else if (shellType == ShellType.Message)
        {
            var messageBlock = MessageManager.Instance.GetMessageBlock(eid);
            if (messageBlock == null)
            {
                return;
            }
            ProcessManager.Instance.InterruptQueueAsync(1);
            draggingProcessBlock = ProcessManager.Instance.GetProcessByPid(messageBlock.BSId);
            draggingPopup = messageBlock.Popup;
        }
        else if (shellType == ShellType.FileDialog) 
        {
            var fileDialogBlock = FileDialogManager.Instance.GetFileDialogBlock(eid);
            if (fileDialogBlock == null) 
            {
                return;
            }
            ProcessManager.Instance.InterruptQueueAsync(1);
            draggingProcessBlock = ProcessManager.Instance.GetProcessByPid(fileDialogBlock.BPId);
            draggingPopup = fileDialogBlock.Popup;
        }
        initialPointerOffset = new Point(
            initialPointerPosition.X - draggingPopup.HorizontalOffset,
            initialPointerPosition.Y - draggingPopup.VerticalOffset);
    }
}
