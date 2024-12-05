using Windows.UI;

namespace OperatingSystemSimulator.Hardware;

public enum HardwareStatuses
{
    Running,
    Waiting,
    Idle
}

public static class HardwareStatusesExtensions
{
    public static SolidColorBrush GetBrush(this HardwareStatuses status)
    {
        return status switch
        {
            HardwareStatuses.Running => new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)),
            HardwareStatuses.Waiting => new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
            HardwareStatuses.Idle => new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
