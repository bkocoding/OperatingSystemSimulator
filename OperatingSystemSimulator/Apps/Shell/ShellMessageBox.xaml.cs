using System.Xml.XPath;

namespace OperatingSystemSimulator.Apps.Shell;

public sealed partial class ShellMessageBox : Page
{

    public int BPid
    {
        get;
        set;
    }

    private int _mid;
    public int Mid
    {
        get => _mid;
        set
        {
            _mid = value;
            ShellTitleBar.EId = _mid;
        }
    }

    private MessageBlock _parentBlock;
    public ShellMessageBox(int mid, int bPid, string message, string title, bool HasCancel, MessageBlock parentBlock)
    {
        InitializeComponent();
        ShellTitleBar.title = title;
        ShellTitleBar.IsApp = false;
        Message.Text = message;
        Mid = mid;
        BPid = bPid;
        _parentBlock = parentBlock;
        OkButton.Click += (s, e) => parentBlock.HandleOk();
        if (HasCancel)
        {
            CancelButton.Visibility = Visibility.Visible;
            CancelButton.Click += (s, e) => parentBlock.HandleCancel();
        }
    }
}
