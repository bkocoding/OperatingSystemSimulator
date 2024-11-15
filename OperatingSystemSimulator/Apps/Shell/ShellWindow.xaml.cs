using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;

namespace OperatingSystemSimulator.Apps.Shell
{
    public sealed partial class ShellWindow : UserControl
    {
        public int Pid { get; set; }

        private string? _title;
        private Popup popupInstance;

        private bool isDragging = false;
        private Point initialPopupPosition;

        public string? title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    titleText.Text = _title;
                }
            }
        }

        public ShellWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessManager.Instance.TerminateProcess(Pid, TerminateReasons.Self);
        }

        private void Pointer_Pressed(object sender, PointerRoutedEventArgs e)
        {
            if (popupInstance == null)
            {
                popupInstance = ProcessManager.Instance.GetProcessByPid(Pid).Popup;
            }
            isDragging = true;

        }

        private void Pointer_Released(object sender, PointerRoutedEventArgs e)
        {

        }

       
    }
}
