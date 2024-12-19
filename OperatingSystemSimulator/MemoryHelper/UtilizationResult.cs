namespace OperatingSystemSimulator.MemoryHelper;
public enum UtilizationResult
{
    Success,
    OutOfMemory
}

public static class UtilizationResultExtensions
{
    private static readonly Dictionary<UtilizationResult, string> Descriptions = new()
    {
        { UtilizationResult.Success, "UTILIZATION_SUCCESS" },
        { UtilizationResult.OutOfMemory, "OUT_OF_MEMORY" }
    };

    public static string GetDescription(this UtilizationResult result)
    {
        return Descriptions.TryGetValue(result, out var description)
            ? description
            : "UNKNOWN";
    }
}
