namespace OperatingSystemSimulator.Models;
public record BIOSSettings
{
    public required string FirstBootOption { get; set; }
    public required string SecondBootOption { get; set; }
    public bool CMOSReset { get; set; }
}
