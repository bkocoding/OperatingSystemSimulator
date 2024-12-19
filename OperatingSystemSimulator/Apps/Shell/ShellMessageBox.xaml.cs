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

    /// <summary>
    /// For creating a message box with all buttons and all button customized names.
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="bPid"></param>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="OKButtonText"></param>
    /// <param name="notOKButtonText"></param>
    /// <param name="cancelButtonText"></param>
    /// <param name="parentBlock"></param>
    public ShellMessageBox(int mid, int bPid, string message, string title, string OKButtonText, string notOKButtonText, string cancelButtonText, MessageBlock parentBlock)
    {
        InitializeComponent();
        ShellTitleBar.title = title;
        ShellTitleBar.IsApp = false;
        Message.Text = message;
        Mid = mid;
        BPid = bPid;
        _parentBlock = parentBlock;

        OkButton.Content = OKButtonText;
        OkButton.Click += (s, e) => parentBlock.HandleOk();

        NotOkButton.Content = notOKButtonText;
        NotOkButton.Visibility = Visibility.Visible;
        NotOkButton.Click += (s, e) => parentBlock.HandleNotOK();

        CancelButton.Content = cancelButtonText;
        CancelButton.Visibility = Visibility.Visible;
        CancelButton.Click += (s, e) => parentBlock.HandleCancel();
    }

    /// <summary>
    /// For creating a message box with only OK button and default OK button name.
    /// </summary>
    /// <param name="mid"></param>
    /// <param name="bPid"></param>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="parentBlock"></param>
    public ShellMessageBox(int mid, int bPid, string message, string title, MessageBlock parentBlock) 
    {
        InitializeComponent();
        ShellTitleBar.title = title;
        ShellTitleBar.IsApp = false;
        Message.Text = message;
        Mid = mid;
        BPid = bPid;
        _parentBlock = parentBlock;

        OkButton.Click += (s, e) => parentBlock.HandleOk();
    }

}
