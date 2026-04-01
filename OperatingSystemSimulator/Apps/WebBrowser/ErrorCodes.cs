namespace OperatingSystemSimulator.Apps.WebBrowser;
public enum ErrorCodes
{
    ERR_INTERNET_DISCONNECTED,
    ERR_NAME_NOT_RESOLVED,
    ERR_OUT_OF_MEMORY,
    ERR_NO_ERROR
}

public static class ErrorCodesExtensions
{
    private static readonly Dictionary<ErrorCodes, string> Descriptions = new()
    {
        { ErrorCodes.ERR_INTERNET_DISCONNECTED, "You are not connected." },
        { ErrorCodes.ERR_NAME_NOT_RESOLVED, "The address could not be resolved." },
        { ErrorCodes.ERR_OUT_OF_MEMORY, "Out of memory.\nYou need more resources in order to visit a website." },
        { ErrorCodes.ERR_NO_ERROR, "No error.\nIt appears that you intentionally visited the browser's default error page." }
    };

    public static string GetDescription(this ErrorCodes errorCode)
    {
        return Descriptions.TryGetValue(errorCode, out var description)
            ? description
            : "Unknown error.";
    }
}
