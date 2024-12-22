using Microsoft.UI.Xaml.Controls.Primitives;

namespace OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
public class MessageBlock
{
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;
    public TaskCompletionSource<MessageResults> MessageResult { get; } = new();

    public int MId { get; }
    public int BPId { get; set; }
    public Popup? Popup { get; set; }
    public ShellMessageBox? MessageBox { get; set; }
    public string Message { get; }
    public string Title { get; }
    public bool HasCancelButton { get; set; }
    public bool HasNotOkButton { get; set; }

    public MessageBlock(int mid, int bPid, string title, string message, string OKButtonText, string notOKButtonText, string cancelButtonText)
    {
        MId = mid;
        BPId = bPid;
        Message = message;
        Title = title;
        HasCancelButton = true;
        HasNotOkButton = true;
        Popup = new();
        MessageBox = new(mid, bPid, message, title, OKButtonText, notOKButtonText, cancelButtonText, this);
    }

    public MessageBlock(int mid, int bPid, string title, string message)
    {
        MId = mid;
        BPId = bPid;
        Message = message;
        Title = title;
        HasCancelButton = false;
        HasNotOkButton = false;
        Popup = new();
        MessageBox = new(mid, bPid, message, title, this);
    }

    public void Show()
    {

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

        Popup.HorizontalOffset = (Window.Current.Bounds.Width - newWidthOffset) / 2;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 2;
        Popup.IsOpen = true;
    }
    public void HandleOk()
    {
        MessageResult.TrySetResult(MessageResults.OK);
        MessageManager.Instance.Close(MId);
    }

    public void HandleNotOK()
    {
        MessageResult.TrySetResult(MessageResults.NotOK);
        MessageManager.Instance.Close(MId);
    }

    public void HandleCancel()
    {
        MessageResult.TrySetResult(MessageResults.Cancelled);
        MessageManager.Instance.Close(MId);
    }


}
