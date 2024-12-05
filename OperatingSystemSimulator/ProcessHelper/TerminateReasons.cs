namespace OperatingSystemSimulator.ProcessHelper;

public enum TerminateReasons
{
    Self,
    User,
    System,
    Unexpected
}

public static class TerminateReasonsExtensions
{
    private static readonly Dictionary<TerminateReasons, string> Descriptions = new()
    {
        { TerminateReasons.Self, "Requested by application itself." },
        { TerminateReasons.User, "Requested by user." },
        { TerminateReasons.System, "Requested by system." },
        { TerminateReasons.Unexpected, "Application terminated unexpectedly." }
    };

    public static string GetDescription(this TerminateReasons reason)
    {
        return Descriptions.TryGetValue(reason, out var description)
            ? description
            : "Unknown termination reason.";
    }
}
