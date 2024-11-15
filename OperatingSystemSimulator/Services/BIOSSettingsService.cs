using System.Text.Json;
using OperatingSystemSimulator.Extras.ConsoleLogger;

namespace OperatingSystemSimulator.Services
{

    public class BIOSSettingsService
    {
        private readonly string _biossettingsFilePath = "biossettings.json";
        private FileSystemWatcher _watcher = new FileSystemWatcher();
        public BIOSSettings Settings { get; set; }

        private Frame rootFrame;

        public BIOSSettingsService()
        {
            LoadSettings();
            WatchFile();
            rootFrame = Window.Current.Content as Frame;
        }

        private void LoadSettings()
        {
            try
            {
                var json = File.ReadAllText(_biossettingsFilePath);
                Settings = JsonSerializer.Deserialize<BIOSSettings>(json);

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
                ConsoleLogger.Log("Trying to reset CMOS...",LogType.Warning);
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
                Path = fileInfo.DirectoryName,
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
}
