namespace OperatingSystemSimulator.Hardware;

public sealed partial class HardwarePage : Page
{
    private readonly HardwarePageViewModel ViewModel;
    public HardwarePage()
    {
        InitializeComponent();
        ViewModel = HardwarePageViewModel.Instance;
        DataContext = ViewModel;
    }


}
