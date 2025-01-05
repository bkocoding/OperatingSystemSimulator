using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OperatingSystemSimulator.Apps.Shell.FileDialogs;
public sealed partial class SelectFileDialog : UserControl, INotifyPropertyChanged
{
    public int BPid
    {
        get;
        set;
    }

    private int _did;
    public int Did
    {
        get => _did;
        set
        {
            _did = value;
            ShellTitleBar.EId = _did;
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

    private BKOFSDirectory? selectedDir;
    private BKOFSFile? selectedFile;

    private Stack<BKOFSDirectory> directoryHistory = new();

    public event PropertyChangedEventHandler? PropertyChanged;

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

    private bool isSelectingFile;
    private bool isNameNeeded;
    private FileDialogBlock _parentBlock;

    public SelectFileDialog(int did, int bPid, bool isSelectingFile, bool isNameNeeded, FileDialogBlock parentBlock)
    {
        InitializeComponent();
        DataContext = this;
        Did = did;
        BPid = bPid;
        this.isSelectingFile = isSelectingFile;
        this.isNameNeeded = isNameNeeded;
        FileNameTextBox.MaxLength = BKOFSManager.MaxNameSize;
        _parentBlock = parentBlock;
        ShellTitleBar.Title = (isSelectingFile ? "Select a File" : "Select a Folder");
        ShellTitleBar.CurrentShellType = ShellType.FileDialog;
        _currentDirectory = BKOFSManager.Instance.RootDirectory;
        OnCurrentDirectoryChanged();

        if (!isSelectingFile)
        {
            OkButton.IsEnabled = true;
            if (isNameNeeded) 
            {
                FileNameTextBox.IsReadOnly = false;
                FileNameTextBox.PlaceholderText = "Type a name";
            }
        }


    }

    private void FileDialogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        bool isItemSelected = FileDialogList.SelectedItem != null;
        if (isSelectingFile && isItemSelected)
        {
            var selectedFileSystemItem = (FileSystemItemModel)FileDialogList.SelectedItem!;
            if (selectedFileSystemItem.Type == "File")
            {
                selectedDir = null;
                selectedFile = (BKOFSFile)selectedFileSystemItem.Content;
                OkButton.Content = "Select";
            }
            else
            {
                OkButton.Content = "Enter";
                selectedFile = null;
                selectedDir = null;
            }
            OkButton.IsEnabled = true;
        }
        else if (!isSelectingFile && isItemSelected)
        {
            var selectedFileSystemItem = (FileSystemItemModel)FileDialogList.SelectedItem!;
            if (selectedFileSystemItem.Type == "File")
            {
                selectedDir = CurrentDirectory;
                selectedFile = null;
                OkButton.Content = "Select";
                OkButton.IsEnabled = true;
            }
            else
            {
                selectedFile = null;
                selectedDir = (BKOFSDirectory)selectedFileSystemItem.Content;
                OkButton.Content = "Select";
                OkButton.IsEnabled = true;
            }
        }
        else if (!isSelectingFile)
        {
            selectedDir = CurrentDirectory;
            selectedFile = null;
            OkButton.Content = "Select";
            OkButton.IsEnabled = true;
        }
        else
        {
            selectedDir = null;
            selectedFile = null;
            OkButton.Content = "Select";
            OkButton.IsEnabled = false;
        }
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(BPid);
        GoBack();
    }

    private async void UpButton_Click(object sender, RoutedEventArgs e)
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(BPid);
        GoUp();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(BPid);
        UpdateFileSystemItems();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {

        if (isSelectingFile)
        {
            if ((string)OkButton.Content == "Enter")
            {
                var selectedFileSystemItem = (FileSystemItemModel)FileDialogList.SelectedItem!;
                ChangeDirectory((BKOFSDirectory)selectedFileSystemItem.Content, false);
                return;
            }

            _parentBlock.HandleSelect(selectedFile!);
        }
        else
        {
            if (FileNameTextBox.Text.Length <= 0 && isNameNeeded)
            {
                MessageManager.Instance.CreateMessage(BPid, "Error", "File name can't be empty!", ShellType.FileDialog);
            }
            else
            {
                if (selectedDir == null)
                {
                    selectedDir = CurrentDirectory;
                }
                _parentBlock.HandleSelect(selectedDir, FileNameTextBox.Text);
            }

        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _parentBlock.HandleCancel();
    }

    private async void FileDialogList_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var selectedItem = (FileSystemItemModel)FileDialogList.SelectedItem;
        if (selectedItem != null && selectedItem.Type == "Directory")
        {
            var directory = (BKOFSDirectory)selectedItem.Content;
            if (directory.IsRestricted)
            {
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
                await Task.Delay(100);
                MessageManager.Instance.CreateMessage(BPid, "Access Denied", $"You don't have access to directory {directory.Name}!", ShellType.FileDialog);
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            }
            else
            {
                ChangeDirectory(directory, false);
            }
        }
        if(selectedItem != null && selectedItem.Type == "File" && isSelectingFile)
        {
            var file = (BKOFSFile)selectedItem.Content;
            if (file.IsRestricted)
            {
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
                await Task.Delay(100);
                MessageManager.Instance.CreateMessage(BPid, "Access Denied", $"You don't have access to file {file.Name}!", ShellType.FileDialog);
                HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
                HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            }
            else 
            {
                OkButton_Click(sender, e);
            }
        }
    }


    private async void UpdateFileSystemItems()
    {
        var directories = CurrentDirectory.ChildDirectories.Select(dir => new FileSystemItemModel
        {
            Type = "Directory",
            Name = dir.Name,
            CreatedAt = dir.CreatedAt,
            LastChanged = dir.LastChanged,
            Size = BKOFSManager.FormatSize(dir.GetTotalSize()),
            Id = dir.DirID,
            Content = dir
        });

        var files = CurrentDirectory.Files.Select(file => new FileSystemItemModel
        {
            Type = "File",
            Name = file.Name,
            CreatedAt = file.CreatedAt,
            LastChanged = file.LastChanged,
            Size = BKOFSManager.FormatSize(file.Size),
            Id = file.FileID,
            Content = file
        });
        await Task.Delay(200);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
        await Task.Delay(100);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
        BKOFSManager.ValidateDirectoryAndFiles(CurrentDirectory);
        FileSystemItems = new ObservableCollection<FileSystemItemModel>(directories.Concat(files));
    }

    private void OnCurrentDirectoryChanged()
    {
        //HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        //HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
        //await Task.Delay(100);
        PathTextBox.Text = CurrentDirectory.GetPath();
        BackButton.IsEnabled = directoryHistory.Count > 0;
        UpButton.IsEnabled = CurrentDirectory.ParentDirectoryID != null;
        UpdateFileSystemItems();
        //HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        //HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
    }


    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        if (CurrentDirectory.ParentDirectoryID != null)
        {
            ChangeDirectory(BKOFSManager.Instance.GetDirectoryById(CurrentDirectory.ParentDirectoryID ?? BKOFSManager.Instance.RootDirectory.DirID), false);
        }
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

}
