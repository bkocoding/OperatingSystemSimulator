
using Newtonsoft.Json;
using OperatingSystemSimulator.Apps.Shell.FileDialogs;
using OperatingSystemSimulator.Apps.Shell.MessageBoxHelper;
using OperatingSystemSimulator.Converters;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.FileHelper;
using OperatingSystemSimulator.NetworkHelper;
using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.Services;

namespace OperatingSystemSimulator.Pages;
public sealed partial class BugCheckPage : Page
{
    private BIOSSettingsService _biosSettingsService = (Application.Current as App)!.Host!.Services.GetRequiredService<BIOSSettingsService>()!;
    private string[] BugCheckParameters = [];
    private readonly List<ProcessBlock> ProcessBlocks = ProcessManager.Instance.ProcessBlocks.ToList();
    public BugCheckPage()
    {
        HardwarePageViewModel.Instance.BugCheckStatusesChange();
        Task.Delay(1000).Wait();
        InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string[] bugCheckParameters)
        {
            BugCheckParameters = bugCheckParameters;
            ConsoleLogger.Log($"A BugCheck has been started, Reason: {BugCheckParameters[2]}", LogType.Warning);
            BugCheckText.Text = "Your Computer ran into a problem and needs to restart.\n\n" +
            $"Reason: {BugCheckParameters[2]}\nCaused by: {BugCheckParameters[0]}\n{BugCheckParameters[1]}";
            MessageManager.Instance.TerminateAllMessages();
            FileDialogManager.Instance.TerminateAllFileDialogs();
            ProcessManager.Instance.TerminateAllProcesses(TerminateReasons.Unexpected);
            NetworkManager.Instance.DisconnectForcefully();
            _ = UpdateProgressBar();
        }
        else
        {
            ConsoleLogger.Log($"A BugCheck has been started, Reason: UNHANDLED_ERROR", LogType.Warning);
            BugCheckText.Text = "Your Computer ran into a problem and needs to restart.\n\n" +
            "Reason: UNHANDLED_ERROR";
            _ = UpdateProgressBar();
        }
    }

    private async Task UpdateProgressBar()
    {
        int duration = 5000;
        int interval = 50;
        int steps = duration / interval;

        for (int i = 0; i <= steps; i++)
        {
            BugCheckProgress.Value = (i * 100) / steps;
            await Task.Delay(interval);
        }

        await Task.Delay(1000);
        await CreateDumpFile();
        ConsoleLogger.Log("BugCheck completed, rebooting...", LogType.Info);

        if (BugCheckParameters[2] == "FILE_SYSTEM_ERROR")
        {
            _biosSettingsService.SaveLastBootState(false);
        }

        await ProcessManager.Instance.TerminateProcess(1, TerminateReasons.System, false);

        ConsoleLogger.Log("Kernel is terminated, PID: 1, Reason: Requested by system.", LogType.Warning);

        Frame.Navigate(typeof(BootPage));

    }

    private async Task CreateDumpFile()
    {
        var bugCheckDirectory = BKOFSManager.Instance.RootDirectory.ChildDirectories.FirstOrDefault(d => d.Name == "Dump");
        if (bugCheckDirectory == null)
        {
            var dirCreationResult = await BKOFSManager.Instance.CreateDirectory("Dump", BKOFSManager.Instance.RootDirectory, false, false);
            if (dirCreationResult)
            {
                bugCheckDirectory = BKOFSManager.Instance.RootDirectory.ChildDirectories.FirstOrDefault(d => d.Name == "Dump");
            }
            else
            {
                ConsoleLogger.Log("Failed to dump memory!", LogType.Error);
            }
        }
        if (bugCheckDirectory != null)
        {
            string fileName = "BC.txt";
            int fileIndex = 1;
            while (bugCheckDirectory.Files.Any(f => f.Name == fileName))
            {
                fileName = $"BC({fileIndex}).txt";
                fileIndex++;
            }
            var fileCreationResult = await BKOFSManager.Instance.CreateFile(fileName, bugCheckDirectory, false, needsPrinting: false);

            if (fileCreationResult)
            {
                BugCheckFile data = new()
                {
                    Reason = BugCheckParameters[2],
                    CausedBy = BugCheckParameters[0] + ", " + BugCheckParameters[1],
                    MemoryDump = ProcessBlocks.ToList()
                };

                var file = bugCheckDirectory.Files.FirstOrDefault(f => f.Name == fileName);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter> { new CustomTypeNameConverter() }
                };

                await BKOFSManager.ChangeFile(file!.FileID, bugCheckDirectory, newContent: JsonConvert.SerializeObject(data, settings), needsPrinting: false);
            }
        }
    }
}
