using System.Collections.ObjectModel;
using System.ComponentModel;
using OperatingSystemSimulator.FileHelper;

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
        directoryHistory.Push(CurrentDirectory);
        CurrentDirectory = newDirectory;
        OnCurrentDirectoryChanged();
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
        if (CurrentDirectory.ParentDirectory != null)
        {
            ChangeDirectory(CurrentDirectory.ParentDirectory);
        }
    }

    private void OnCurrentDirectoryChanged()
    {
        BackButtonEnabled = directoryHistory.Count > 0;
        UpButtonEnabled = CurrentDirectory.ParentDirectory != null;

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
            Size = CalculateDirectorySize(dir),
            Content = dir
        });

        var files = CurrentDirectory.Files.Select(file => new FileSystemItemModel
        {
            Type = "File",
            Name = file.Name,
            CreatedAt = file.CreatedAt,
            LastChanged = file.LastChanged,
            Size = file.Size,
            Content = file
        });

        FileSystemItems = new ObservableCollection<FileSystemItemModel>(directories.Concat(files));
    }

    private int CalculateDirectorySize(BKOFSDirectory directory)
    {
        return directory.Files.Sum(file => file.Size) +
               directory.ChildDirectories.Sum(subDir => CalculateDirectorySize(subDir));
    }
}
