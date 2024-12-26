using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
public class MessageBlock
{
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;

    private ShellType shellType;

    public TaskCompletionSource<MessageResults> MessageResult { get; } = new();

    public int MId { get; }
    public int BSId { get; set; }
    public Popup? Popup { get; set; }
    public ShellMessageBox? MessageBox { get; set; }
    public string Message { get; }
    public string Title { get; }
    public bool HasCancelButton { get; set; }
    public bool HasNotOkButton { get; set; }

    public MessageBlock(int mId, int bSId, string title, string message, string OKButtonText, string notOKButtonText, string cancelButtonText, ShellType shellType)
    {
        MId = mId;
        BSId = bSId;
        this.shellType = shellType;
        Message = message;
        Title = title;
        HasCancelButton = true;
        HasNotOkButton = true;
        Popup = new();
        MessageBox = new(mId, bSId, message, title, OKButtonText, notOKButtonText, cancelButtonText, this);
    }

    public MessageBlock(int mId, int bPid, string title, string message, ShellType shellType)
    {
        MId = mId;
        BSId = bPid;
        this.shellType = shellType;
        Message = message;
        Title = title;
        HasCancelButton = false;
        HasNotOkButton = false;
        Popup = new();
        MessageBox = new(mId, bPid, message, title, this);
    }

    public void Show()
    {
        ConsoleLogger.Log($"Process {BSId}, requested to create a message. Message ID: {MId}.", LogType.MessageBox);

        Popup!.Child = MessageBox;
        double newWidthOffset;
        double newHeightOffset;

        do
        {
            newWidthOffset = 300;
        } while (newWidthOffset == previousWidthOffset);

        do
        {
            newHeightOffset = 300;
        } while (newHeightOffset == previousHeightOffset);

        previousWidthOffset = newWidthOffset;
        previousHeightOffset = newHeightOffset;

        Popup.HorizontalOffset = (Window.Current!.Bounds.Width - newWidthOffset) / 2;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 2;
        Popup.IsOpen = true;
    }
    public void HandleOk()
    {
        BringToFront();
        MessageResult.TrySetResult(MessageResults.OK);
        ConsoleLogger.Log($"Message {MId} returned MessageResult.OK.", LogType.Result);
        MessageManager.Instance.Close(MId);
    }

    public void HandleNotOK()
    {
        BringToFront();
        MessageResult.TrySetResult(MessageResults.NotOK);
        ConsoleLogger.Log($"Message {MId} returned MessageResult.NotOK.", LogType.Result);
        MessageManager.Instance.Close(MId);
    }

    public void HandleCancel()
    {
        BringToFront();
        ConsoleLogger.Log($"Message {MId} returned MessageResult.Cancelled.", LogType.Result);
        MessageResult.TrySetResult(MessageResults.Cancelled);
        MessageManager.Instance.Close(MId);
    }

    private void BringToFront()
    {
        if (shellType == ShellType.Message)
        {
            MessageManager.Instance.BringToFront(BSId);
        }
        else if (shellType == ShellType.FileDialog)
        {
            FileDialogManager.Instance.BringToFront(BSId);
        }
        else if (shellType == ShellType.App)
        {
            ProcessManager.Instance.BringToFront(BSId);
        }
    }

}
