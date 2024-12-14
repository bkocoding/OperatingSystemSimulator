using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Background;

namespace OperatingSystemSimulator.Apps.Shell;
public class MessageBlock
{
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;
    public TaskCompletionSource<MessageResults> MessageResult { get; } = new();

    public int Mid { get; }
    public int BPid { get; set; }
    public Popup? Popup { get; set; }
    public ShellMessageBox? MessageBox { get; set; }
    public string Message { get; }
    public string Title { get; }
    public bool HasCancelButton { get; set; }

    public MessageBlock(int mid, int bPid, string title, string message, bool hasCancel)
    {
        Mid = mid;
        BPid = bPid;
        Message = message;
        Title = title;
        HasCancelButton = hasCancel;
        Popup = new();
        MessageBox = new(mid, bPid, message, title, hasCancel, this);
    }
    public void Show()
    {

        Popup.Child = MessageBox;
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
        MessageManager.Instance.Close(Mid);
    }

    public void HandleCancel()
    {
        MessageResult.TrySetResult(MessageResults.Cancelled);
        MessageManager.Instance.Close(Mid);
    }

    public void HandleClosed()
    {
        MessageResult.TrySetResult(MessageResults.Closed);
        MessageManager.Instance.Close(Mid);
    }
}
