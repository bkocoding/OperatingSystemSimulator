using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Extras.ConsoleLogger;
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
    private bool isNameTextBoxVisible = false;

    private Stack<BKOFSDirectory> directoryHistory = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public FileExplorerApp()
    {
        InitializeComponent();
        DataContext = this;
        NameTextBox.MaxLength = BKOFSManager.MaxNameSize;
        ProcessManager.Instance.FocusedPopupChanged += OnFocusedPopupChanged;
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
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);

        if (!isNameTextBoxVisible)
        {
            isNameTextBoxVisible = true;
            NameTextBox.Visibility = Visibility.Visible;
            ActivateTypingToNameTextBox();
            return;
        }

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", "Please enter a name for the file!", ShellType.App);
            return;
        }

        string fileName = NameTextBox.Text;
        var result = await BKOFSManager.Instance.CreateFile(fileName, CurrentDirectory);
        if (result)
        {
            isNameTextBoxVisible = false;
            NameTextBox.Visibility = Visibility.Collapsed;
            NameTextBox.Text = "";
            UpdateFileSystemItems();
        }
        else
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", "A file with the same name already exists in this directory!", ShellType.App);
        }
    }

    private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
    {

        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);
        if (!isNameTextBoxVisible)
        {
            isNameTextBoxVisible = true;
            NameTextBox.Visibility = Visibility.Visible;
            ActivateTypingToNameTextBox();
            return;
        }

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", "Please enter a name for the directory!", ShellType.App);
            return;
        }

        string directoryName = NameTextBox.Text;

        var result = await BKOFSManager.Instance.CreateDirectory(directoryName, CurrentDirectory, false);

        if (result)
        {
            isNameTextBoxVisible = false;
            NameTextBox.Visibility = Visibility.Collapsed;
            NameTextBox.Text = "";
            UpdateFileSystemItems();
        }
        else
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", "A directory with the same name already exists in this directory!", ShellType.App);
        }

    }

    private async void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        await ProcessManager.Instance.EnqueueRunningProcessAsync(Pid);

        if (!isNameTextBoxVisible)
        {
            isNameTextBoxVisible = true;
            NameTextBox.Visibility = Visibility.Visible;
            ActivateTypingToNameTextBox();
            return;
        }

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
        }
        else
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", "Please select a file or directory to rename!", ShellType.App);
            return;
        }

        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageManager.Instance.CreateMessage(Pid, "Error", $"Please enter a name for the selected {(isDirectory ? "directory" : "file")}!", ShellType.App);
            return;
        }

        string newName = NameTextBox.Text;

        if (isDirectory)
        {
            var result = await BKOFSManager.Instance.RenameDirectory(newName, selectedDirectory!);
            if (result)
            {
                isNameTextBoxVisible = false;
                NameTextBox.Visibility = Visibility.Collapsed;
                NameTextBox.Text = "";
                UpdateFileSystemItems();
            }
            else
            {
                isNameTextBoxVisible = false;
                NameTextBox.Visibility = Visibility.Collapsed;
                NameTextBox.Text = "";
            }

        }
        else
        {
            var result = await BKOFSManager.ChangeFile(selectedFile!.FileID, CurrentDirectory, newName);
            if (result)
            {
                isNameTextBoxVisible = false;
                NameTextBox.Visibility = Visibility.Collapsed;
                NameTextBox.Text = "";
                UpdateFileSystemItems();
            }
            else
            {
                isNameTextBoxVisible = false;
                NameTextBox.Visibility = Visibility.Collapsed;
                NameTextBox.Text = "";
            }
        }

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
                    $"Are you sure you want to permanently delete this {(isDirectory ? "directory" : "file")}?\n\nName: {(isDirectory ? selectedDirectory!.Name : selectedFile!.Name)}\nSize: {(isDirectory ? BKOFSManager.FormatSize(selectedDirectory!.GetTotalSize()) : BKOFSManager.FormatSize(selectedFile!.Size))}\nCreated at: {(isDirectory ? selectedDirectory!.CreatedAt : selectedFile!.CreatedAt)}\nLast Changed at: {(isDirectory ? selectedDirectory!.LastChanged : selectedFile!.LastChanged)}", "Yes", "No", "Cancel", ShellType.App);
                lastMessageID = messageBlock.MId;
                messageExists = true;
                MessageResults? result = await messageBlock.MessageResult.Task;

                if (result == MessageResults.OK)
                {
                    messageExists = false;
                    if (isDirectory)
                    {
                        bool resultOfDirectoryDeletion = await BKOFSManager.Instance.DeleteDirectory(selectedFileSystemItem.Id, CurrentDirectory!);
                        if (resultOfDirectoryDeletion)
                        {
                            if (directoryHistory.Contains(selectedDirectory!))
                            {
                                RemoveDirectoryAndSubdirectoriesFromHistory(selectedDirectory!);
                            }
                            UpdateFileSystemItems();
                        }

                    }
                    else if (!isDirectory)
                    {
                        bool resultOfFileDeletion = await BKOFSManager.DeleteFile(selectedFileSystemItem.Id, CurrentDirectory);
                        if (resultOfFileDeletion)
                        {

                            UpdateFileSystemItems();
                        }
                    }
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
                MessageManager.Instance.CreateMessage(Pid, "Access Denied", $"You don't have access to directory {directory.Name}!", ShellType.App);
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
            await BKOFSManager.TryOpeningFile(file);
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

    private void RemoveDirectoryAndSubdirectoriesFromHistory(BKOFSDirectory deletedDirectory)
    {
        var directoriesToRemove = new HashSet<BKOFSDirectory>();
        CollectAllSubdirectories(deletedDirectory, directoriesToRemove);

        Stack<BKOFSDirectory> tempStack = new();

        while (directoryHistory.Count > 0)
        {
            var dir = directoryHistory.Pop();
            if (!directoriesToRemove.Contains(dir) && dir != CurrentDirectory)
            {
                tempStack.Push(dir);
            }
        }

        while (tempStack.Count > 0)
        {
            directoryHistory.Push(tempStack.Pop());
        }
    }

    private static void CollectAllSubdirectories(BKOFSDirectory directory, HashSet<BKOFSDirectory> directoriesToRemove)
    {
        directoriesToRemove.Add(directory);

        foreach (var child in directory.ChildDirectories)
        {
            CollectAllSubdirectories(child, directoriesToRemove);
        }
    }

    private void ActivateTypingToNameTextBox()
    {
        NameTextBox.Focus(FocusState.Programmatic);
        NameTextBox.SelectionStart = NameTextBox.Text.Length;

    }

    private void OnFocusedPopupChanged(Popup? focusedPopup)
    {

        if (ProcessManager.Instance.GetProcessByPid(Pid) == null)
        {
            UnsubscribeToFocusedPopUpChangedEvent();
            return;
        }

        if (focusedPopup != ProcessManager.Instance.GetProcessByPid(Pid)!.Popup)
        {
            NameTextBox.IsEnabled = false;
        }
        else
        {
            NameTextBox.IsEnabled = true;
        }
    }

    public void UnsubscribeToFocusedPopUpChangedEvent()
    {
        ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;
    }

    public async void ChangeDirectory(BKOFSDirectory newDirectory, bool isSentbyBackButton)
    {
        if (BKOFSManager.Instance.GetDirectoryById(newDirectory.DirID) != null)
        {
            if (!isSentbyBackButton)
            {
                directoryHistory.Push(CurrentDirectory);
            }
            CurrentDirectory = newDirectory;
            OnCurrentDirectoryChanged();
        }
        else
        {
            ConsoleLogger.Log($"Couldn't load the directory {newDirectory.Name}, Reason: Directory does not exist.", LogType.Error);
            var messageBlock = MessageManager.Instance.CreateMessage(1, "Error", $"Couldn't load the directory {newDirectory.Name}. Directory does not exists.", ShellType.None);
            var result = await messageBlock.MessageResult.Task;
            UpdateFileSystemItems();
        }


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

    private void OnCurrentDirectoryChanged()
    {
        //HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        //HardwarePageViewModel.Instance.SetHDOperation(HDOperations.ExploringDirectory);
        //await Task.Delay(100);
        PathTextBox.Text = CurrentDirectory.GetPath();
        BackButton.IsEnabled = directoryHistory.Count > 0;
        UpButton.IsEnabled = CurrentDirectory.ParentDirectoryID != null;
        SetTitle(CurrentDirectory.Name);
        UpdateFileSystemItems();
        //HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        //HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
    }

    private void SetTitle(string newTitle)
    {
        CurrentTitle = newTitle + " - File Explorer";
        ShellTitleBar.Title = CurrentTitle;
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
        FileSystemItems = new ObservableCollection<FileSystemItemModel>(directories.Concat(files));
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
    }


    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
