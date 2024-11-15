
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.Pages;
public sealed partial class BugCheckPage : Page
{
    public BugCheckPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string[] BugCheckParameters)
        {
            ConsoleLogger.Log($"A BugCheck has been started, Reason: {BugCheckParameters[1]}", LogType.Warning);
            BugCheckText.Text = "Your Computer ran into a problem and needs to restart.\n\n" +
            $"Reason: {BugCheckParameters[1]}\nCaused by: {BugCheckParameters[0]}";
            ProcessManager.Instance.TerminateAllProcesses(TerminateReasons.Unexpected);
            _ = UpdateProgressBar();
        }
        else 
        {
            ConsoleLogger.Log($"A BugCheck has been started, Reason: UNHANDLED_ERROR", LogType.Warning);
            BugCheckText.Text = "Your Computer ran into a problem and needs to restart.\n\n" +
            "Reason: UNHANDLED_ERROR";
            _ = UpdateProgressBar();
        }
    }

    private async Task UpdateProgressBar()
    {
        int duration = 5000;
        int interval = 50;
        int steps = duration / interval;

        for (int i = 0; i <= steps; i++)
        {
            BugCheckProgress.Value = (i * 100) / steps;
            await Task.Delay(interval);
        }
        await Task.Delay(1000);
        Frame.Navigate(typeof(BootPage));
    }
}
