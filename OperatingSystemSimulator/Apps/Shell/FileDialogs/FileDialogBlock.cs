using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.FileHelper;

namespace OperatingSystemSimulator.Apps.Shell.FileDialogs;
public class FileDialogBlock
{
    private double previousWidthOffset = 200;
    private double previousHeightOffset = 200;

    public TaskCompletionSource<FileDialogResult> DialogResult { get; } = new();

    public int DId { get; }
    public int BPId { get; set; }
    public bool IsSelectingFile { get; set; }
    public Popup? Popup { get; set; }
    public SelectFileDialog? FileDialog { get; set; }


    public FileDialogBlock(int dId, int bPId, bool isSelectingFile)
    {
        DId = dId;
        BPId = bPId;
        IsSelectingFile = isSelectingFile;
        Popup = new Popup();
        FileDialog = new(DId, BPId, IsSelectingFile, this);
    }

    public void Show()
    {
        Popup!.Child = FileDialog;

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

        Popup.HorizontalOffset = (Window.Current.Bounds.Width - newWidthOffset) / 4;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 2;
        Popup.IsOpen = true;
    }

    public void HandleSelect(BKOFSFile selectedFile)
    {
        DialogResult.TrySetResult(new FileDialogResult(selectedFile));
    }
    public void HandleSelect(BKOFSDirectory selectedDirectory, string fileName)
    {
        foreach (var file in selectedDirectory.Files)
        {
            if (file.Name == fileName)
            {
                MessageManager.Instance.CreateMessage(BPId, "Error - File Already Exists", $"File {fileName} already exists!");
                return;
            }
        }

        DialogResult.TrySetResult(new FileDialogResult(selectedDirectory, fileName));
        FileDialogManager.Instance.Close(DId);
    }

    public void HandleCancel()
    {
        DialogResult.TrySetResult(new FileDialogResult());
        FileDialogManager.Instance.Close(DId);
    }

}
