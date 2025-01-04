using System.Collections.ObjectModel;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps.Shell.FileDialogs;
public class FileDialogManager
{
    private static FileDialogManager? instance;
    private static readonly object lockObject = new();

    public ObservableCollection<FileDialogBlock> FileDialogBlocks = new();
    private int nextDId = 10;

    public static FileDialogManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new FileDialogManager();
                    }
                }
            }
            return instance;
        }
    }

    public FileDialogBlock CreateFileDialog(int pid, bool isSelectingFile, bool isNameNeeded)
    {
        FileDialogBlock fileDialogBlock = new(nextDId, pid, isSelectingFile, isNameNeeded);
        nextDId++;
        FileDialogBlocks.Add(fileDialogBlock);
        fileDialogBlock.Show();
        BringToFront(fileDialogBlock.DId);
        ProcessManager.Instance.FocusedPopup = null;
        return fileDialogBlock;
    }

    public void Close(int dId)
    {
        FileDialogBlock? fileDialogBlock = GetFileDialogBlock(dId);
        if (fileDialogBlock != null)
        {
            fileDialogBlock.Popup!.IsOpen = false;
            fileDialogBlock.Popup.Child = null;
            fileDialogBlock.Popup = null;
            fileDialogBlock.FileDialog = null;
            FileDialogBlocks.Remove(fileDialogBlock);
            GC.Collect();
        }
    }

    public void BringToFront(int dId)
    {
        FileDialogBlock? fileDialogBlock = GetFileDialogBlock(dId);
        if (fileDialogBlock != null)
        {
            ProcessManager.Instance.FocusedPopup = null;
            fileDialogBlock.Popup!.IsOpen = false;
            fileDialogBlock.Popup.IsOpen = true;
        }
    }

    public FileDialogBlock? GetFileDialogBlock(int dId)
    {
        return FileDialogBlocks.FirstOrDefault(p => p.DId == dId);
    }

    public void TerminateAllFileDialogs() 
    {
        var fileDialogBlocksCopy = FileDialogBlocks.ToList();
        foreach (var fileDialogBlock in fileDialogBlocksCopy) 
        {
            Close(fileDialogBlock.DId);
        }
    }
}
