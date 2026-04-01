using OperatingSystemSimulator.Apps.Enums;
using OperatingSystemSimulator.Apps.Shell.Enums;

namespace OperatingSystemSimulator.ToolTipHelper.ToolTipTools;
public class ToolTipParameters
{
    public ShellType SType { get; set; } = ShellType.App;
    public AppType? AType { get; set; }
    public Dictionary<string, string>? ExtraParams { get; set; }

}
