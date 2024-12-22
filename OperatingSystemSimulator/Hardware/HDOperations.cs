namespace OperatingSystemSimulator.Hardware;

public enum HDOperations
{
    NotMounted,
    Idle,
    OperatingSystem,
    MBR,
    SMART,
    AppData,
    CreatingFile,
    ChangingFile,
    DeletingFile,
    CreatingDirectory,
    DeletingDirectory,
    ChangingDirectory,
    ExploringDirectory
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
            HDOperations.CreatingFile => "Creating File",
            HDOperations.ChangingFile => "Changing File",
            HDOperations.DeletingFile => "Deleting File",
            HDOperations.CreatingDirectory => "Creating Directory",
            HDOperations.DeletingDirectory => "Deleting Directory",
            HDOperations.ChangingDirectory => "Changing Directory",
            HDOperations.ExploringDirectory => "Exploring Directory",
            _ => "Standard Operation"
        };
    }
}
