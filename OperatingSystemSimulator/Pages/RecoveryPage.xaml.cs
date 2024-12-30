using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.FileHelper;
using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.Services;

namespace OperatingSystemSimulator.Pages;

public sealed partial class RecoveryPage : Page
{
    private DispatcherTimer? timer;
    private DispatcherTimer? delayTimer;
    private int dotCount = 0;
    private int iterationCount = 0;
    private const int maxIterations = 3;
    private bool isOSOK = true;
    private BIOSSettingsService _BIOSSettingsService = (Application.Current as App)!.Host!.Services.GetRequiredService<BIOSSettingsService>()!;

    public RecoveryPage()
    {
        InitializeComponent();
        StartDotAnimation();
    }

    private async void StartDotAnimation()
    {
        await Task.Delay(3000);
        Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 9, 95, 161));
        RecoveryText.Visibility = Visibility.Visible;
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        timer.Tick += Timer_Tick!;
        if (!BKOFSManager.Instance.ValidateOS()) 
        {
            isOSOK = false;
        }
        await ProcessManager.Instance.EnqueueRunningProcessAsync(1);
        timer.Start();
    }
    private async void Timer_Tick(object sender, object e)
    {

        if (iterationCount == maxIterations)
        {
            timer!.Stop();
            BKOFSManager.Instance.Recover();
            _BIOSSettingsService.SaveLastBootState(true);
            ProcessManager.Instance.TerminateAllProcesses(TerminateReasons.System);
            await ProcessManager.Instance.TerminateProcess(1, TerminateReasons.System);
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
            HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Idle);
            RecoveryText.Text = "Rebooting...";
            ConsoleLogger.Log("Rebooting system...", LogType.Info);
            StartDelayTimer();
        }

        else if (dotCount < 3)
        {
            RecoveryText.Text += ".";
            dotCount++;
        }
        else
        {

            RecoveryText.Text = "Last boot attempt was unsuccessful, attempting to fix the problem";
            dotCount = 0;
            iterationCount++;
        }
    }

    private void StartDelayTimer()
    {
        delayTimer = new DispatcherTimer();
        delayTimer.Interval = TimeSpan.FromSeconds(1);
        delayTimer.Tick += DelayTimer_Tick!;
        delayTimer.Start();
    }

    private void DelayTimer_Tick(object sender, object e)
    {
        
        Frame.Navigate(typeof(BootPage));
        delayTimer!.Stop();

    }
    

}
