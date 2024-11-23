using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.Pages;

public sealed partial class ShutdownPage : Page
{
    private Timer _timer = null!;
    private Timer _delayTimer = null!;
    private int dotCount = 0;
    private int iterationCount = 0;
    private const int maxIterations = 3;

    public ShutdownPage()
    {
        InitializeComponent();
        restartButton.Visibility = Visibility.Collapsed;
        StartDotAnimation();
    }

    private void StartDotAnimation()
    {
        _timer = new Timer(TimerCallback, null, 0, 500);
    }

    private void TimerCallback(object? state)
    {
        if (iterationCount >= maxIterations)
        {
            sdtext.Text = "";
            _timer.Dispose();
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
            StartDelayTimer();
            return;
        }

        if (dotCount < 3)
        {
            sdtext.Text += ".";
            dotCount++;
        }
        else
        {
            sdtext.Text = "Shutting Down";
            dotCount = 0;
            iterationCount++;
        }
    }

    private void StartDelayTimer()
    {
        _delayTimer = new Timer(DelayTimerCallback, null, 2000, Timeout.Infinite);
    }

    private void DelayTimerCallback(object? state)
    {
        _delayTimer.Dispose();
        ConsoleLogger.Log("Kernel is terminated, PID: 1, Reason: Requested by system.", LogType.Info);
        UpdateUIAfterDelay();
    }

    private async void UpdateUIAfterDelay()
    {
        if (restartButton.Dispatcher.HasThreadAccess)
        {
            restartButton.Visibility = Visibility.Visible;
        }
        else
        {
            await restartButton.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                restartButton.Visibility = Visibility.Visible;
            });
        }
    }

    private void Restartbtn_click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(BootPage));
    }
}

