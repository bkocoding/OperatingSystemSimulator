using Newtonsoft.Json;
using OperatingSystemSimulator.Apps;
using OperatingSystemSimulator.Apps.Shell;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.FileHelper;
public class BKOFSManager
{
    private static BKOFSManager? instance;
    private static readonly object lockObject = new();
    private const string JsonFilePath = "BKOFSData.json";
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
                        instance.LoadFromJson();
                    }
                }
            }
            return instance;
        }
    }

    public BKOFSDirectory RootDirectory { get; private set; } = new BKOFSDirectory("Root", 0);


    private int currentDirID;
    private int currentFileID;
    public const int MaxFileSize = 1000000 - 1;
    public const  int MaxNameSize = 11;
    public const int CharSize = 32;

    public void LoadFromJson()
    {
        if (File.Exists(JsonFilePath))
        {
            string json = File.ReadAllText(JsonFilePath);
            var data = JsonConvert.DeserializeObject<BKOFSData>(json);

            if (data != null)
            {
                RootDirectory = data.RootDirectory ?? new BKOFSDirectory("Root", 0);
                currentDirID = data.CurrentDirID;
                currentFileID = data.CurrentFileID;

                ValidateDirectoryAndFiles(RootDirectory);
            }
            else
            {
                Initialize();
                SaveToJson();
            }
        }
        else
        {
            Initialize();
            SaveToJson();
        }
    }

    private static void ValidateDirectoryAndFiles(BKOFSDirectory directory)
    {
        if (directory.Name.Length > MaxNameSize)
        {
            ConsoleLogger.Log($"Directory name '{directory.Name}' exceeds maximum length of {MaxNameSize} characters.", LogType.Error);
        }

        foreach (var file in directory.Files)
        {
            if (file.Name.Length > MaxNameSize)
            {
                ConsoleLogger.Log($"File name '{file.Name}' in directory '{directory.Name}' exceeds maximum length of {MaxNameSize} characters.", LogType.Error);
            }

            if (file.Size > MaxFileSize)
            {
                ConsoleLogger.Log($"File '{file.Name}' in directory '{directory.Name}' exceeds maximum size of {FormatSize(MaxFileSize)}.", LogType.Error);
            }
        }

        foreach (var childDirectory in directory.ChildDirectories)
        {
            ValidateDirectoryAndFiles(childDirectory);
        }
    }

    public void SaveToJson()
    {
        var data = new BKOFSData
        {
            RootDirectory = RootDirectory,
            CurrentDirID = currentDirID,
            CurrentFileID = currentFileID
        };

        var settings = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(JsonFilePath, json);
    }

    private async Task<bool> CreateDirectoryInternal(string name, BKOFSDirectory parentDirectory, bool isRestricted)
    {

        WritoToHDDOperation(HDOperations.CreatingDirectory);

        if (parentDirectory.ChildDirectories.Any(d => d.Name == name))
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't create directory {name}, Reason: Directory already exists.", LogType.Error);
            return false;
        }

        BKOFSDirectory newDirectory = new(name, currentDirID++)
        {
            ParentDirectoryID = parentDirectory.DirID,
            IsRestricted = isRestricted
        };
        parentDirectory.ChildDirectories.Add(newDirectory);
        await ResetHDStatus(50);
        ConsoleLogger.Log($"Directory {name} created successfully.", LogType.Info);
        return true;

    }

    private async Task<bool> RenameDirectoryInternal(string newName, BKOFSDirectory directoryToChange)
    {

        WritoToHDDOperation(HDOperations.ChangingDirectory);

        if (FindDirectoryById(directoryToChange.DirID) == null)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't rename the directory {directoryToChange.Name}, Reason: Directory not found.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "Error", $"Couldn't rename the directory with ID {directoryToChange.DirID}, Reason: Directory not found.", ShellType.None);
            return false;
        }

        if (directoryToChange.IsRestricted)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't rename the directory {directoryToChange.Name}, Reason: No access to directory.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "No Access", "You don't have access to directory " + directoryToChange.Name + "!", ShellType.None);
            return false;
        }

        if (newName.Length > MaxNameSize)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't rename the directory {directoryToChange.Name}, Reason: Directory name is too long.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "Couldn't Rename Directory", $"Directory name is too long! Maximum directory name length is {MaxNameSize} characters.", ShellType.None);
            return false;
        }

        if (BKOFSManager.Instance.GetDirectoryById(directoryToChange.ParentDirectoryID ?? BKOFSManager.Instance.RootDirectory.DirID)!.ChildDirectories.Any(d => d.Name == newName))
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't rename the directory {directoryToChange.Name}, Reason: A directory with the same name already exists.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "Error", "Directory with name " + newName + " already exists!", ShellType.None);
            return false;
        }

        var oldName = directoryToChange.Name;
        directoryToChange.Name = newName;
        directoryToChange.LastChanged = DateTime.Now.ToLocalTime();

        await ResetHDStatus(50);
        ConsoleLogger.Log($"Directory {oldName} renamed to {newName} successfully.", LogType.Info);
        return true;

    }

    private static async Task<bool> DeleteDirectoryInternal(int dirID, BKOFSDirectory parentDirectory)
    {

        WritoToHDDOperation(HDOperations.DeletingDirectory);

        BKOFSDirectory? directory = parentDirectory.ChildDirectories.FirstOrDefault(d => d.DirID == dirID);
        if (directory == null)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't delete the directory with ID {dirID}, Reason: Directory not found.", LogType.Error);
            return false;
        }

        if (directory.IsRestricted)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't delete the directory {directory.Name}, Reason: No access to directory.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "No Access", "You don't have access to directory " + directory.Name + "!", ShellType.None);
            return false;
        }

        parentDirectory.ChildDirectories.Remove(directory);
        await ResetHDStatus(100);
        ConsoleLogger.Log($"Directory {directory.Name} deleted successfully.", LogType.Info);
        return true;

    }

    private async Task<bool> CreateFileInternal(string name, BKOFSDirectory parentDirectory, bool? isRestricted)
    {
        WritoToHDDOperation(HDOperations.CreatingFile);

        if (parentDirectory.Files.Any(f => f.Name == name))
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't create the file {name}, Reason: A file with the same name already exists.", LogType.Error);

            return false;
        }

        BKOFSFile newFile = new(name, parentDirectory.DirID, currentFileID++, isRestricted);
        parentDirectory.Files.Add(newFile);
        await ResetHDStatus(200);
        ConsoleLogger.Log($"File {name} created successfully.", LogType.Info);
        return true;
    }

    private static async Task<bool> ChangeFileInternal(int fileID, BKOFSDirectory parentDirectory, string? newName, bool? isRestricted, string? newContent)
    {
        WritoToHDDOperation(HDOperations.ChangingFile);

        BKOFSFile? file = parentDirectory.Files.FirstOrDefault(f => f.FileID == fileID);
        if (file == null)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't change the file with ID {fileID}, Reason: File not found.", LogType.Error);
            return false;
        }

        if (isRestricted.HasValue)
        {
            file.IsRestricted = isRestricted.Value;
        }

        if (newName != null)
        {
            if (newName.Length > MaxNameSize)
            {
                await ResetHDStatus(50);
                ConsoleLogger.Log($"Couldn't update the file {file.Name}, Reason: File name is too long!", LogType.Error);
                MessageManager.Instance.CreateMessage(1, "Couldn't Change File", $"File name is too long! Maximum file name length is {MaxNameSize} characters.", ShellType.None);
                return false;
            }

            if (parentDirectory.Files.Any(f => f.Name == newName))
            {
                await ResetHDStatus(50);
                ConsoleLogger.Log($"Couldn't update the file {file.Name}, Reason: A file with the same name already exists!", LogType.Error);
                MessageManager.Instance.CreateMessage(1, "Couldn't Change File", "A file with the same name already exists in this directory!", ShellType.None);
                return false;
            }

            file.Name = newName;
        }



        if (newContent != null)
        {
            if (newContent.Length * CharSize > MaxFileSize)
            {
                await ResetHDStatus(50);
                ConsoleLogger.Log($"Couldn't update the file {file.Name}, Reason: File size limit exceeded.", LogType.Error);
                MessageManager.Instance.CreateMessage(1, "File Size Limit Exceeded", $"Couldn't change the file {file.Name}, file size limit exceeded! Maximum file size is {FormatSize(MaxFileSize)}.", ShellType.None);
                return false;
            }
            file.Content = newContent;
            file.Size = file.Content.Length * CharSize;
        }

        file.LastChanged = DateTime.Now;
        await ResetHDStatus(50);
        ConsoleLogger.Log($"File {file.Name} updated successfully.", LogType.Info);
        return true;
    }

    private static async Task<bool> DeleteFileInternal(int fileID, BKOFSDirectory parentDirectory)
    {
        WritoToHDDOperation(HDOperations.DeletingFile);

        BKOFSFile? file = parentDirectory.Files.FirstOrDefault(f => f.FileID == fileID);
        if (file == null)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't delete the file with ID {fileID}, Reason: File not found.", LogType.Error);
            return false;
        }

        if (file.IsRestricted)
        {
            await ResetHDStatus(50);
            ConsoleLogger.Log($"Couldn't delete the file {file.Name}, Reason: No access to file.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "No Access", "You don't have access to file " + file.Name + "!", ShellType.None);
            return false;
        }

        parentDirectory.Files.Remove(file);
        await ResetHDStatus(100);
        ConsoleLogger.Log($"File {file.Name} deleted successfully.", LogType.Info);
        return true;

    }

    public async Task<bool> CreateDirectory(string name, BKOFSDirectory parentDirectory, bool isRestricted)
    {
        if (await CreateDirectoryInternal(name, parentDirectory, isRestricted))
        {
            SaveToJson();
            return true;
        }
        return false;
    }

    public async Task<bool> RenameDirectory(string newName, BKOFSDirectory directoryToChange)
    {
        if (await RenameDirectoryInternal(newName, directoryToChange))
        {
            SaveToJson();
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteDirectory(int dirID, BKOFSDirectory parentDirectory)
    {
        if (await DeleteDirectoryInternal(dirID, parentDirectory))
        {
            SaveToJson();
            return true;
        }
        return false;
    }

    public async Task<bool> CreateFile(string name, BKOFSDirectory parentDirectory, bool? isRestricted = false)
    {
        if (await CreateFileInternal(name, parentDirectory, isRestricted))
        {
            SaveToJson();
            return true;
        }
        return false;
    }

    public static async Task<bool> ChangeFile(int fileID, BKOFSDirectory parentDirectory, string? newName = null, bool? isRestricted = null, string? newContent = null)
    {
        if (await ChangeFileInternal(fileID, parentDirectory, newName, isRestricted, newContent))
        {
            Instance.SaveToJson();
            return true;
        }
        return false;
    }

    public static async Task<bool> DeleteFile(int fileID, BKOFSDirectory parentDirectory)
    {
        if (await DeleteFileInternal(fileID, parentDirectory))
        {
            Instance.SaveToJson();
            return true;
        }
        return false;
    }


    private static void WritoToHDDOperation(HDOperations operation)
    {
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Running);
        HardwarePageViewModel.Instance.SetHDOperation(operation);
    }

    private static async Task ResetHDStatus(int delay)
    {
        await Task.Delay(delay);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdWrite, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
    }

    public void Initialize()
    {
        currentDirID = 1;
        currentFileID = 1;

        RootDirectory = new BKOFSDirectory("Root", 0);

        string[] directoryNames = { "System", "Documents", "Downloads", "Pictures", "Music", "Videos" };

        foreach (var name in directoryNames)
        {
            BKOFSDirectory newDirectory = new(name, currentDirID++)
            {
                ParentDirectoryID = RootDirectory.DirID,
                IsRestricted = name == "System"
            };

            if (name == "System")
            {
                var file = new BKOFSFile("System", newDirectory.DirID, currentFileID++, true);
                file.Size = MaxFileSize-20;
                newDirectory.Files.Add(file);
            }

            RootDirectory.ChildDirectories.Add(newDirectory);
        }
    }

    public static async Task<bool> TryOpeningFile(BKOFSFile file)
    {
        if (file.IsRestricted)
        {
            ConsoleLogger.Log($"Couldn't open the file {file.Name}, Reason: No access to file.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "No Access", "You don't have access to file " + file.Name + "!", ShellType.None);
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
                ConsoleLogger.Log($"Couldn't open the file {file.Name}, Reason: No suitable handler found for the file.", LogType.Error);
                MessageManager.Instance.CreateMessage(1, "No Handler Found", "No suitable handler found for file " + file.Name + ".", ShellType.None);
            }
        }
        return false;
    }


    public static string FormatSize(int size)
    {
        if (size < 1000)
        {
            return size + " B";
        }
        else if (size < 1000 * 1000)
        {
            return (size / 1000) + " KB";
        }
        else if (size < 1000 * 1000 * 1000)
        {
            return (size / 1000 / 1000) + " MB";
        }
        else
        {
            return (size / 1000 / 1000 / 1000) + " GB";
        }
    }
    public BKOFSDirectory? GetDirectoryById(int dID)
    {
        return FindDirectoryById(RootDirectory, dID);
    }

    private static BKOFSDirectory? FindDirectoryById(int dID)
    {
        return FindDirectoryById(Instance.RootDirectory, dID);
    }

    private static BKOFSDirectory? FindDirectoryById(BKOFSDirectory currentDirectory, int dID)
    {
        if (currentDirectory.DirID == dID)
        {
            return currentDirectory;
        }

        foreach (var childDirectory in currentDirectory.ChildDirectories)
        {
            var result = FindDirectoryById(childDirectory, dID);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

}
