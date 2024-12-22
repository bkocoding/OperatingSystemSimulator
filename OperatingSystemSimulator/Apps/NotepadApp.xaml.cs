using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.FileHelper;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Apps;

public sealed partial class NotepadApp : UserControl
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

    private string _title = "";
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            SetTitle(_title);
        }
    }

    private BKOFSFile? currentFile;

    private bool isChanged = false;
    private bool isFirstUtilizationWithFile = false;
    private bool isFile = false;
    private bool exitMessageExists = false;
    private bool wrappingEnabled = true;
    private bool isEnqueued = false;
    private int? lastMessageID;
    public NotepadApp(BKOFSFile file)
    {
        InitializeComponent();
        currentFile = file;
        Title = file.Name;
        isFile = true;
        isFirstUtilizationWithFile = true;
        NotepadTextBox.Text = file.Content;
        SaveButton.IsEnabled = true;
        ProcessManager.Instance.FocusedPopupChanged += OnFocusedPopupChanged;
    }

    public NotepadApp()
    {
        InitializeComponent();
        Title = "Untitled";
        ProcessManager.Instance.FocusedPopupChanged += OnFocusedPopupChanged;
    }

    public void SetTitle(string title)
    {
        ShellTitleBar.Title = title + " - Notepad";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        var result = await SaveChanges();
        if (result)
        {
            SetTitle(Title);
            isChanged = false;
        }
    }

    private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        var result = await SaveAs();
        if (result)
        {
            isChanged = false;
            isFile = true;
        }
    }

    private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        ActivateTyping();
    }

    private void NotepadTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
        ActivateTyping();
    }

    private async void NotepadTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isFirstUtilizationWithFile)
        {
            isFirstUtilizationWithFile = false;
            return;
        }

        if (!isChanged)
        {
            SetTitle("* " + Title);
            isChanged = true;
        }
        await ProcessManager.Instance.InterruptQueueAsync(Pid);
        await Task.Delay(30);
    }

    private void ActivateTyping()
    {
        NotepadTextBox.Focus(FocusState.Programmatic);
        NotepadTextBox.SelectionStart = NotepadTextBox.Text.Length;

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
            NotepadTextBox.IsEnabled = false;
        }
        else
        {
            NotepadTextBox.IsEnabled = true;
        }
    }

    public async void TryTerminate()
    {
        if (isChanged && !exitMessageExists)
        {
            var messageBlock = MessageManager.Instance.CreateMessage(Pid, "Warning - Notepad", "Do you want to save changes?", "Save", "Don't Save", "Cancel");
            lastMessageID = messageBlock.MId;
            exitMessageExists = true;
            MessageResults? result = await messageBlock.MessageResult.Task;

            if (result == MessageResults.OK)
            {
                exitMessageExists = false;
                if (isFile)
                {
                    var saveChangesResult = await SaveChanges();
                    if (!saveChangesResult)
                    {
                        return;
                    }
                }
                else
                {
                    var saveAsResult = await SaveAs();
                    if (!saveAsResult)
                    {
                        return;
                    }
                }
                OnTermination();
            }
            else if (result == MessageResults.NotOK)
            {
                exitMessageExists = false;
                OnTermination();
            }
            else if (result == MessageResults.Cancelled)
            {
                exitMessageExists = false;
            }
        }
        else
        {
            if (!exitMessageExists)
            {
                OnTermination();
            }
            else
            {
                MessageManager.Instance.BringToFront(lastMessageID!.Value);
            }
        }
    }

    private void OnTermination()
    {
        ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;
        ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
    }

    private async Task<bool> SaveChanges()
    {
        if (!isChanged)
        {
            return true;
        }

        var result = await BKOFSManager.ChangeFile(currentFile!.FileID, currentFile!.ParentDirectory, newContent: NotepadTextBox.Text);

        if (!result)
        {
            MessageManager.Instance.CreateMessage(Pid, "Couldn't Save", $"The current file {currentFile.Name} does not exists anymore!");
            Title = "Untitled";
            SetTitle("* " + Title);
            isChanged = true;
            currentFile = null;
            isFile = false;
            SaveButton.IsEnabled = false;
        }

        return result;
    }

    private async Task<bool> SaveAs()
    {
        var fileDialogBlock = FileDialogManager.Instance.CreateFileDialog(Pid, false);

        FileDialogResult? fileDialogResult = await fileDialogBlock.DialogResult.Task;

        BKOFSDirectory selectedDir;

        if (fileDialogResult.Result == FileDialogResults.Cancelled)
        {
            return false;
        }
        else if (fileDialogResult.Result == FileDialogResults.DirectorySelected)
        {
            selectedDir = fileDialogResult.SelectedDirectory!;
            await BKOFSManager.Instance.CreateFile(fileDialogResult.SelectedName!, selectedDir);
            bool result = await BKOFSManager.ChangeFile(selectedDir.Files.Last().FileID, selectedDir, newContent: NotepadTextBox.Text);

            if (result)
            {
                currentFile = selectedDir.Files.Last();
                Title = currentFile.Name;
                SetTitle(Title);
            }

            if (!SaveButton.IsEnabled && result)
            {
                SaveButton.IsEnabled = true;
            }

            return result;
        }
        return false;
    }

    public void UnsubscribeToFocusedPopUpChangedEvent()
    {
        ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;
    }

    ~NotepadApp()
    {
        ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;

    }

    private void WrappingButton_Click(object sender, RoutedEventArgs e)
    {
        if (wrappingEnabled)
        {
            wrappingEnabled = false;
            WrappingButton.Content = "No Wrap";
            NotepadTextBox.TextWrapping = TextWrapping.NoWrap;

        }
        else
        {
            wrappingEnabled = true;
            WrappingButton.Content = "Wrap";
            NotepadTextBox.TextWrapping = TextWrapping.Wrap;
        }
        var currentText = NotepadTextBox.Text;
        NotepadTextBox.Text = string.Empty;
        NotepadTextBox.Text = currentText;

    }
}
