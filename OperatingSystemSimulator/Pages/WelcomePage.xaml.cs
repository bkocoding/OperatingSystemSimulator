using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.Pages;
public sealed partial class WelcomePage : Page
{
    private DispatcherTimer timer;
    private DispatcherTimer delayTimer;
    private int dotCount = 0;
    private int iterationCount = 0;
    private const int maxIterations = 3;
    public WelcomePage()
    {
        InitializeComponent();
        StartDotAnimation();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private async void StartDotAnimation()
    {
        await Task.Delay(1000);

        Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 9, 95, 161));
        wctext.Visibility = Visibility.Visible;

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(250);
        timer.Tick += Timer_Tick;
        ConsoleLogger.Log("Desktop is loading", LogType.Info);
        timer.Start();
    }

    private void Timer_Tick(object sender, object e)
    {

        if (iterationCount == maxIterations)
        {
            timer.Stop();
            ConsoleLogger.Log("Desktop is loaded", LogType.Info);
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
            HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            wctext.Text = "Welcome";
            StartDelayTimer();
        }

        else if (dotCount < 3)
        {
            wctext.Text += ".";
            dotCount++;
        }
        else
        {
            wctext.Text = "Loading Desktop";
            dotCount = 0;
            iterationCount++;
        }
    }

    private void StartDelayTimer()
    {
        delayTimer = new DispatcherTimer();
        delayTimer.Interval = TimeSpan.FromSeconds(1);
        delayTimer.Tick += DelayTimer_Tick;
        delayTimer.Start();
    }

    private void DelayTimer_Tick(object sender, object e)
    {
        Frame.Navigate(typeof(DesktopPage));
        delayTimer.Stop();

    }
}
