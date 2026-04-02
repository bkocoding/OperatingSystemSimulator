namespace OperatingSystemSimulator.ProcessHelper;
using System.Timers;
public class ProcessManagerScheduler
{
    private static Timer? runServiceTimer;

    public static void StartRunServiceScheduler()
    {

        runServiceTimer = new Timer(120000);

        runServiceTimer.Elapsed += (sender, e) => ProcessManager.Instance.RunService(2);

        runServiceTimer.AutoReset = true;
        runServiceTimer.Enabled = true;

    }

    public static void StopRunServiceScheduler()
    {
        if (runServiceTimer != null)
        {
            runServiceTimer.Stop();
            runServiceTimer.Dispose();
            runServiceTimer = null;
        }
    }
}
