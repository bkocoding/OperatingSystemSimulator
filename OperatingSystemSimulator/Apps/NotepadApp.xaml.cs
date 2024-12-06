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
            ShellTitleBar.Pid = _pid;
        }
    }

    private string _title = "";
    public string Title {
        get => _title;
        set
        {
            _title = value;
            SetTitle(_title);
        }
    }

    private bool isChanged = false;
    public NotepadApp(string title)
    {
        InitializeComponent();
        Title = title;
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
    }

    private void NotepadTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        ProcessManager.Instance.BringToFront(Pid);

    }

    private void NotepadTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isChanged)
        {
            SetTitle("* " + Title);
            isChanged = true;
        }
    }
}
