using OperatingSystemSimulator.Apps.Enums;

namespace OperatingSystemSimulator.Apps.Interfaces;

public interface IApp
{
    int Pid { get; set; }
    AppType ApplicationType { get; }
}
