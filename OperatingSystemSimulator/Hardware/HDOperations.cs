namespace OperatingSystemSimulator.Hardware;

public enum HDOperations
{
    NotMounted,
    Idle,
    OperatingSystem,
    MBR,
    SMART,
    AppData

}

public static class HDOperationsExtensions
{
    public static string GetDescription(this HDOperations operation)
    {
        return operation switch
        {
            HDOperations.NotMounted => "Disk Not Mounted",
            HDOperations.Idle => "Idle",
            HDOperations.OperatingSystem => "Operating System",
            HDOperations.MBR => "Master Boot Record",
            HDOperations.SMART => "S.M.A.R.T. Status",
            HDOperations.AppData => "Application Data",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
}
