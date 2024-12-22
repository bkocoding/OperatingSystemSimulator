using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.FileHelper;
using OperatingSystemSimulator.ProcessHelper;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OperatingSystemSimulator.Apps;
public partial class FileExplorerApp : UserControl, INotifyPropertyChanged
{
    private int _pid;
    public int Pid
    {
        get => _pid;
        set
        {
            _pid = value;
            ShellTitleBar.EId = _pid;
        }
    }

    private BKOFSDirectory _currentDirectory;
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

    private int? lastMessageID;
    private bool messageExists;

    private Stack<BKOFSDirectory> directoryHistory = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public FileExplorerApp()
    {
        InitializeComponent();
        DataContext = this;

        _currentDirectory = BKOFSManager.Instance.RootDirectory;
        OnCurrentDirectoryChanged();
        ShellTitleBar.Title = CurrentTitle;
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        GoBack();
    }

    private async void UpButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        GoUp();
    }

    private async void NewFileButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO: IMPLEMENT
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        string fileName = "New File.txt";
        await BKOFSManager.Instance.CreateFile(fileName, CurrentDirectory);
        UpdateFileSystemItems();
    }

    private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
    {
        //TODO: Implement
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        await BKOFSManager.Instance.CreateDirectory("New Folder", CurrentDirectory, false);
        UpdateFileSystemItems();
    }

    private async void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        //TODO: Implement
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        //await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        if (messageExists)
        {
            MessageManager.Instance.BringToFront(lastMessageID!.Value);
        }
        else
        {
            bool isDirectory = false;
            BKOFSDirectory? selectedDirectory = null;
            BKOFSFile? selectedFile = null;
            var selectedFileSystemItem = (FileSystemItemModel)ExplorerList.SelectedItem;
            if (selectedFileSystemItem != null)
            {
                if (selectedFileSystemItem.Type == "Directory")
                {
                    isDirectory = true;
                    selectedDirectory = (BKOFSDirectory)selectedFileSystemItem.Content;
                }
                else
                {
                    selectedFile = (BKOFSFile)selectedFileSystemItem.Content;
                }



                var messageBlock = MessageManager.Instance.CreateMessage(Pid, $"Delete {(isDirectory ? "Directory" : "File")}",
                    $"Are you sure you want to permanently delete this {(isDirectory ? "directory" : "file")}?\n\nName: {(isDirectory ? selectedDirectory!.Name : selectedFile!.Name)}\nSize: {(isDirectory ? CalculateDirectorySize(selectedDirectory!) : selectedFile!.Size)}\nCreated at: {(isDirectory ? selectedDirectory!.CreatedAt : selectedFile!.CreatedAt)}\nLast Changed at: {(isDirectory ? selectedDirectory!.LastChanged : selectedFile!.LastChanged)}", "Yes", "No", "Cancel");
                lastMessageID = messageBlock.MId;
                messageExists = true;
                MessageResults? result = await messageBlock.MessageResult.Task;

                if (result == MessageResults.OK)
                {
                    messageExists = false;
                    if (isDirectory)
                    {
                        bool resultOfDirectoryDeletion = await BKOFSManager.Instance.DeleteDirectory(selectedFileSystemItem.Id, CurrentDirectory!);
                    }
                    else if (!isDirectory)
                    {
                        bool resultOfFileDeletion = await BKOFSManager.DeleteFile(selectedFileSystemItem.Id, CurrentDirectory);
                    }
                    UpdateFileSystemItems();
                }
                else if (result == MessageResults.NotOK || result == MessageResults.Cancelled)
                {
                    messageExists = false;
                }
            }
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        UpdateFileSystemItems();
    }

    private async void ExplorerList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var selectedItem = (FileSystemItemModel)ExplorerList.SelectedItem;
        if (selectedItem != null && selectedItem.Type == "Directory")
        {
            var directory = (BKOFSDirectory)selectedItem.Content;
            if (directory.IsRestricted)
            {
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
                await Task.Delay(100);
                MessageManager.Instance.CreateMessage(Pid, "Access Denied", $"You don't have access to directory {directory.Name}!");
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            }
            else
            {
                ChangeDirectory(directory, false);
            }
        }
        else if (selectedItem != null && selectedItem.Type == "File")
        {
            var file = (BKOFSFile)selectedItem.Content;
            await BKOFSManager.Instance.TryOpeningFile(file);
        }
    }

    private async void ExplorerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        bool isItemSelected = ExplorerList.SelectedItem != null;
        RenameButton.IsEnabled = isItemSelected;
        DeleteButton.IsEnabled = isItemSelected;
    }

    public void ChangeDirectory(BKOFSDirectory newDirectory, bool isSentbyBackButton)
    {
        if (!isSentbyBackButton)
        {
            directoryHistory.Push(CurrentDirectory);
        }
        CurrentDirectory = newDirectory;
        OnCurrentDirectoryChanged();
    }

    public void GoBack()
    {
        if (directoryHistory.Count > 0)
        {
            ChangeDirectory(directoryHistory.Pop(), true);
        }
    }

    public void GoUp()
    {
        if (CurrentDirectory.ParentDirectory != null)
        {
            ChangeDirectory(CurrentDirectory.ParentDirectory, false);
        }
    }

    private async void OnCurrentDirectoryChanged()
    {
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
        await Task.Delay(100);
        BackButton.IsEnabled = directoryHistory.Count > 0;
        UpButton.IsEnabled = CurrentDirectory.ParentDirectory != null;
        SetTitle(CurrentDirectory.Name);
        UpdateFileSystemItems();
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
    }

    private void SetTitle(string newTitle)
    {
        CurrentTitle = newTitle + " - File Explorer";
        ShellTitleBar.Title = CurrentTitle;
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
            Id = dir.DirID,
            Content = dir
        });

        var files = CurrentDirectory.Files.Select(file => new FileSystemItemModel
        {
            Type = "File",
            Name = file.Name,
            CreatedAt = file.CreatedAt,
            LastChanged = file.LastChanged,
            Size = file.Size,
            Id = file.FileID,
            Content = file
        });

        FileSystemItems = new ObservableCollection<FileSystemItemModel>(directories.Concat(files));
    }

    private static int CalculateDirectorySize(BKOFSDirectory directory)
    {
        return directory.Files.Sum(file => file.Size) +
               directory.ChildDirectories.Sum(subDir => CalculateDirectorySize(subDir));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
