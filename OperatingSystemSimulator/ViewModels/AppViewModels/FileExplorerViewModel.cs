using System.Collections.ObjectModel;
using System.ComponentModel;
using OperatingSystemSimulator.Apps.Shell.Enums;

namespace OperatingSystemSimulator.ViewModels.AppViewModels;

public class FileExplorerViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private BKOFSDirectory _currentDirectory = BKOFSManager.Instance.RootDirectory;

    public BKOFSDirectory CurrentDirectory
    {
        get => _currentDirectory;
        set
        {
            if (_currentDirectory != value)
            {
                _currentDirectory = value;
                OnCurrentDirectoryChanged();
            }
        }
    }

    private ObservableCollection<FileSystemItemModel> _fileSystemItems = new();
    public ObservableCollection<FileSystemItemModel> FileSystemItems
    {
        get => _fileSystemItems;
        set
        {
            _fileSystemItems = value;
            OnPropertyChanged(nameof(FileSystemItems));
        }
    }

    private string _currentTitle = "";
    public string CurrentTitle
    {
        get => _currentTitle;
        private set
        {
            if (_currentTitle != value)
            {
                _currentTitle = value;
                OnPropertyChanged(nameof(CurrentTitle));
            }
        }
    }

    private Stack<BKOFSDirectory> directoryHistory = new();

    public bool UpButtonEnabled = false;
    public bool BackButtonEnabled = false;


    public FileExplorerViewModel()
    {
        SetTitle(CurrentDirectory.Name);
    }

    public void ChangeDirectory(BKOFSDirectory newDirectory)
    {
        if (BKOFSManager.Instance.GetDirectoryById(newDirectory.DirID) != null)
        {
            directoryHistory.Push(CurrentDirectory);
            CurrentDirectory = newDirectory;
            OnCurrentDirectoryChanged();
        }
        else
        {
            ConsoleLogger.Log($"Couldn't load the directory {newDirectory.Name}, Reason: Directory does not exist.", LogType.Error);
            MessageManager.Instance.CreateMessage(1, "Error", $"Couldn't load the directory {newDirectory.Name}. Directory does not exists.", ShellType.None);
            UpdateFileSystemItems();
        }

    }

    public void GoBack()
    {
        if (directoryHistory.Count > 0)
        {
            ChangeDirectory(directoryHistory.Pop());
        }
    }

    public void GoUp()
    {
        if (CurrentDirectory.ParentDirectoryID != null)
        {
            BKOFSManager.Instance.GetDirectoryById(CurrentDirectory.ParentDirectoryID ?? BKOFSManager.Instance.RootDirectory.DirID);
        }
    }

    private void OnCurrentDirectoryChanged()
    {
        BackButtonEnabled = directoryHistory.Count > 0;
        UpButtonEnabled = CurrentDirectory.ParentDirectoryID != null;

        SetTitle(CurrentDirectory.Name);

        UpdateFileSystemItems();
    }

    private void SetTitle(string newTitle)
    {
        CurrentTitle = newTitle + " - File Explorer";
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateFileSystemItems()
    {
        var directories = CurrentDirectory.ChildDirectories.Select(dir => new FileSystemItemModel
        {
            Type = "Directory",
            Name = dir.Name,
            CreatedAt = dir.CreatedAt,
            LastChanged = dir.LastChanged,
            Size = BKOFSManager.FormatSize(dir.GetTotalSize()),
            Content = dir
        });

        var files = CurrentDirectory.Files.Select(file => new FileSystemItemModel
        {
            Type = "File",
            Name = file.Name,
            CreatedAt = file.CreatedAt,
            LastChanged = file.LastChanged,
            Size = BKOFSManager.FormatSize(file.Size),
            Content = file
        });

        FileSystemItems = new ObservableCollection<FileSystemItemModel>(directories.Concat(files));
    }

}
