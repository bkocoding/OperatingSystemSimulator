using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.FileHelper;
public class BKOFSManager
{
    private static BKOFSManager? instance;
    private static readonly object lockObject = new();

    public static BKOFSManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new BKOFSManager();
                    }
                }
            }
            return instance;
        }
    }

    public BKOFSDirectory RootDirectory { get; private set; } = new BKOFSDirectory("Root", 0);


    private int currentDirID = 1;
    private int currentFileID = 1;


    public async Task<bool> CreateDirectory(string name, BKOFSDirectory parentDirectory, bool isRestricted)
    {
        SetHDOperation(HDOperations.CreatingDirectory);

        if (parentDirectory.ChildDirectories.Any(d => d.Name == name))
        {
            return false;
        }

        BKOFSDirectory newDirectory = new(name, currentDirID++)
        {
            ParentDirectory = parentDirectory,
            IsRestricted = isRestricted
        };
        parentDirectory.ChildDirectories.Add(newDirectory);
        await ResetHDStatus(50);
        return true;
    }

    public async Task<bool> RenameDirectory(string newName, BKOFSDirectory directoryToChange)
    {
        SetHDOperation(HDOperations.ChangingDirectory);
        directoryToChange.Name = newName;
        directoryToChange.LastChanged = DateTime.Now.ToLocalTime();

        await ResetHDStatus(50);
        return true;
    }

    public async Task<bool> DeleteDirectory(int dirID, BKOFSDirectory parentDirectory)
    {
        SetHDOperation(HDOperations.DeletingDirectory);

        BKOFSDirectory? directory = parentDirectory.ChildDirectories.FirstOrDefault(d => d.DirID == dirID);
        if (directory == null)
        {
            await ResetHDStatus(50);
            return false;
        }

        parentDirectory.ChildDirectories.Remove(directory);
        await ResetHDStatus(100);
        return true;
    }

    public async Task<bool> CreateFile(string name, BKOFSDirectory parentDirectory, bool? isRestricted = false)
    {
        SetHDOperation(HDOperations.CreatingFile);

        if (parentDirectory.Files.Any(f => f.Name == name))
        {
            await ResetHDStatus(50);
            return false;
        }

        BKOFSFile newFile = new BKOFSFile(name, parentDirectory, currentFileID++, isRestricted);
        parentDirectory.Files.Add(newFile);
        await ResetHDStatus(200);

        return true;
    }

    public static async Task<bool> ChangeFile(int fileID, BKOFSDirectory parentDirectory, string? newName = null, bool? isRestricted = null, string? newContent = null)
    {
        SetHDOperation(HDOperations.ChangingFile);

        BKOFSFile? file = parentDirectory.Files.FirstOrDefault(f => f.FileID == fileID);
        if (file == null)
        {
            await ResetHDStatus(50);
            return false;
        }

        file.IsBusy = true;

        if (newName != null)
            file.Name = newName;
        if (isRestricted != null)
            file.IsRestricted = isRestricted.Value;
        if (newContent != null)
        {
            file.Content = newContent;
            file.Size = newContent.Length * 8;
        }

        file.LastChanged = DateTime.Now.ToLocalTime();

        file.IsBusy = false;

        await ResetHDStatus(200);

        return true;
    }

    public static async Task<bool> DeleteFile(int fileID, BKOFSDirectory parentDirectory)
    {
        SetHDOperation(HDOperations.DeletingFile);

        BKOFSFile? file = parentDirectory.Files.FirstOrDefault(f => f.FileID == fileID);
        if (file == null)
        {
            await ResetHDStatus(50);
            return false;
        }
        file.IsBusy = true;

        parentDirectory.Files.Remove(file);

        file.IsBusy = false;
        await ResetHDStatus(100);
        return true;
    }

    private static void SetHDOperation(HDOperations operation)
    {
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Running);
        HardwarePageViewModel.Instance.SetHDOperation(operation);
    }

    private static async Task ResetHDStatus(int delay)
    {
        await Task.Delay(delay);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Idle);
    }

    public async void Initialize()
    {
        await CreateDirectory("System", RootDirectory, true);
        await CreateDirectory("Documents", RootDirectory, false);
        await CreateDirectory("Downloads", RootDirectory, false);
        await CreateDirectory("Pictures", RootDirectory, false);
        await CreateDirectory("Music", RootDirectory, false);
        await CreateDirectory("Videos", RootDirectory, false);
    }

    public async Task<bool> TryOpeningFile(BKOFSFile file)
    {
        if (file.IsRestricted)
        {
            MessageManager.Instance.CreateMessage(1, "No Access", "You don't have access to file " + file.Name + "!");
            return false;
        }
        else
        {
            if (file.Name.EndsWith(".txt"))
            {
                await ProcessManager.Instance.CreateProcess(new NotepadApp(file), "Notepad", false, false);
                return true;
            }
            else
            {
                MessageManager.Instance.CreateMessage(1, "No Handler Found", "No suitable handler found for file " + file.Name + ".");
            }
        }
        return false;
    }

}
