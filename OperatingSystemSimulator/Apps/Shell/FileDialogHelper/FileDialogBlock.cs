using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell.Enums;

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


    public FileDialogBlock(int dId, int bPId, bool isSelectingFile, bool isNameNeeded)
    {
        DId = dId;
        BPId = bPId;
        IsSelectingFile = isSelectingFile;
        Popup = new Popup();
        FileDialog = new(DId, BPId, IsSelectingFile, isNameNeeded, this);
    }

    public void Show()
    {
        ConsoleLogger.Log($"Process {BPId}, requested to create a {(IsSelectingFile ? "Select File Dialog" : "Select Folder Dialog")}. Dialog ID: {DId}.", LogType.FileDialog);

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

        Popup.HorizontalOffset = (Window.Current!.Bounds.Width - newWidthOffset) / 4;
        Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 4;
        Popup.IsOpen = true;
    }

    public void HandleSelect(BKOFSFile selectedFile)
    {
        ProcessManager.Instance.BringToFront(BPId);
        ConsoleLogger.Log($"File Dialog {DId} returned FileDialogResult.FileSeleted. Selected File ID: {selectedFile.FileID}, Name: {selectedFile.Name}.", LogType.Result);
        DialogResult.TrySetResult(new FileDialogResult(selectedFile));
        FileDialogManager.Instance.Close(DId);
    }
    public void HandleSelect(BKOFSDirectory selectedDirectory, string fileName)
    {
        foreach (var file in selectedDirectory.Files)
        {
            if (file.Name == fileName)
            {
                MessageManager.Instance.CreateMessage(DId, "Error - File Already Exists", $"File {fileName} already exists!", ShellType.FileDialog);
                return;
            }
        }
        ProcessManager.Instance.BringToFront(BPId);
        ConsoleLogger.Log($"File Dialog {DId} returned FileDialogResult.DirectorySelected. Selected Directory ID: {selectedDirectory.DirID}, Name: {selectedDirectory.Name}", LogType.Result);
        DialogResult.TrySetResult(new FileDialogResult(selectedDirectory, fileName));
        FileDialogManager.Instance.Close(DId);
    }

    public void HandleCancel()
    {
        ProcessManager.Instance.BringToFront(BPId);
        ConsoleLogger.Log($"File Dialog {DId} returned FileDialogResult.Cancelled.", LogType.Result);
        DialogResult.TrySetResult(new FileDialogResult());
        FileDialogManager.Instance.Close(DId);
    }

}
