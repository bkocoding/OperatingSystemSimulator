namespace OperatingSystemSimulator.Models;
public record BIOSSettings
{
    public string FirstBootOption { get; set; } = "";
    public string SecondBootOption { get; set; } = "";
    public bool CMOSReset { get; set; }
    public bool WasLastBootSuccesful { get; set; }
}
