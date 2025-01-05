namespace OperatingSystemSimulator.Apps.Shell.FileDialogHelper;
public class FileDialogResult
{
    public FileDialogResults Result { get; set; }
    public BKOFSDirectory? SelectedDirectory { get; set; }
    public BKOFSFile? SelectedFile { get; set; }
    public string? SelectedName { get; set; }

    public FileDialogResult(BKOFSDirectory directory, string? selectedName)
    {
        SelectedDirectory = directory;
        Result = FileDialogResults.DirectorySelected;
        SelectedName = selectedName;
    }

    public FileDialogResult(BKOFSFile file)
    {
        SelectedFile = file;
        Result = FileDialogResults.FileSeleted;
    }

    public FileDialogResult()
    {
        Result = FileDialogResults.Cancelled;
    }
}


public enum FileDialogResults
{
    FileSeleted,
    DirectorySelected,
    Cancelled
}
