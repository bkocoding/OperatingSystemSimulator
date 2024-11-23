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
    public static string GetDescription(this TerminateReasons reason)
    {
        return reason switch
        {
            TerminateReasons.Self => "Requested by application itself.",
            TerminateReasons.User => "Requested by user.",
            TerminateReasons.System => "Requested by system.",
            TerminateReasons.Unexpected => "Application terminated unexpectedly.",
            _ => "Unknown termination reason."
        };
    }
}
