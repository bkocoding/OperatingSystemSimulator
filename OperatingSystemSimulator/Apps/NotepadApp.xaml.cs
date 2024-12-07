using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.Apps.Shell;
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

    private bool isChanged = false;
    private bool isFile = false;
    private bool exitMessageExists = false;
    private int? lastMessageID;
    public NotepadApp(string title)
    {
        InitializeComponent();
        Title = title;
        isFile = true;
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
        ShellTitleBar.title = title + " - Notepad";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
    }

    private void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);
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

    private void NotepadTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isChanged)
        {
            SetTitle("* " + Title);
            isChanged = true;
        }
    }

    private void ActivateTyping()
    {
        NotepadTextBox.Focus(FocusState.Programmatic);
        NotepadTextBox.SelectionStart = NotepadTextBox.Text.Length;

    }

    private void OnFocusedPopupChanged(Popup? focusedPopup)
    {

        if (focusedPopup != ProcessManager.Instance.GetProcessByPid(Pid).Popup)
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
            var messageBlock = MessageManager.Instance.CreateMessage(Pid, "Notepad", "Do you want to save changes?", true);
            lastMessageID = messageBlock.Mid;
            exitMessageExists = true;
            bool? result = await messageBlock.MessageResult.Task;

            if (result == true)
            {
                exitMessageExists = false;
                //TODO: SaveChanges() Logic
                ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;
                ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
            }
            else if (result == false)
            {
                exitMessageExists = false;
                ProcessManager.Instance.FocusedPopupChanged -= OnFocusedPopupChanged;
                ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
            }
            else
            {
                exitMessageExists = false;
            }
        }
        else
        {
            if (!exitMessageExists)
            {
                ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
            }
            else { 
                MessageManager.Instance.BringToFront(lastMessageID.Value);
            }
        }
    }

}
