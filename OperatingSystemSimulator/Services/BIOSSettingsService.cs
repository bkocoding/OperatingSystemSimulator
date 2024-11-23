using System.Text.Json;
using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.Services;


public class BIOSSettingsService
{
    private readonly string _biossettingsFilePath = "biossettings.json";
    private FileSystemWatcher _watcher = new();
    public BIOSSettings Settings { get; set; }

    private readonly Frame? rootFrame;

    public BIOSSettingsService()
    {
        Settings = new BIOSSettings
        {
            FirstBootOption = string.Empty,
            SecondBootOption = string.Empty,
            CMOSReset = false
        };
        LoadSettings();
        WatchFile();
        rootFrame = Window.Current?.Content as Frame;
        if (rootFrame == null)
        {
            ConsoleLogger.Log("BIOS Settings Service: Root frame is not set.", LogType.Error);
            throw new InvalidOperationException("BIOS Settings Service: Root frame is not set.");
        }
    }

    private void LoadSettings()
    {
        try
        {
            var json = File.ReadAllText(_biossettingsFilePath);
            var settings = JsonSerializer.Deserialize<BIOSSettings>(json);

            if (settings == null)
            {
                ConsoleLogger.Log("Deserialized BIOS settings are null.", LogType.Error);
                throw new InvalidOperationException("Deserialized BIOS settings are null.");
            }

            Settings = settings;

            if (Settings.CMOSReset)
            {
                CMOSReset();
            }

        }
        catch (Exception ex)
        {
            ConsoleLogger.Log($"Failed to load BIOS settings: {ex.Message}", LogType.Error);
            ConsoleLogger.Log("Trying to reset CMOS...", LogType.Warning);
            CMOSReset();
        }
    }

    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(Settings);
            File.WriteAllText(_biossettingsFilePath, json);
            ConsoleLogger.Log("BIOS settings successfully saved.", LogType.Info);
            LoadSettings();
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log($"Failed to save BIOS settings: {ex.Message}", LogType.Error);
            ConsoleLogger.Log("Trying to reset CMOS...", LogType.Warning);
            CMOSReset();
        }
    }

    public void CMOSReset()
    {
        ConsoleLogger.Log("CMOS Reset Triggered!", LogType.Warning);
        ConsoleLogger.Log("Trying to reset BIOS Settings...", LogType.Warning);
        Settings.CMOSReset = false;
        Settings.FirstBootOption = "Simulated Operating System";
        Settings.SecondBootOption = "Network Boot";
        SaveSettings();
    }

    private void WatchFile()
    {
        var fileInfo = new FileInfo(_biossettingsFilePath);
        _watcher = new FileSystemWatcher
        {
            Path = fileInfo.DirectoryName ?? throw new InvalidOperationException("Bios Settings Service: Directory Name is null."),
            Filter = fileInfo.Name,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };

        _watcher.Changed += (sender, e) =>
        {
            Task.Delay(100).ContinueWith(_ => LoadSettings());
        };

        _watcher.EnableRaisingEvents = true;
    }
}
