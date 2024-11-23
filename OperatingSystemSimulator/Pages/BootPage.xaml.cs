using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.MemoryHelper;
using OperatingSystemSimulator.Services;
using Windows.System;
using Windows.UI.Core;
using MemoryManager = OperatingSystemSimulator.MemoryHelper.MemoryManager;

namespace OperatingSystemSimulator.Pages;
public sealed partial class BootPage : Page
{
    private readonly DispatcherTimer _timer;
    public bool isBusy = true;
    private bool isEnteringBIOS = false;
    private int _messageIndex = 0;
    private string[] _currentMessages;
    private string _firstBootOrder;

    private readonly BIOSSettingsService _biosSettingsService;

    private readonly string[] _postMessages =
    [
            "*** Testing RAM...",
            $"RAM OK! Size: {MemoryManager.memorySize} Byte(s)",
    ];

    private readonly string[] _beforeBootMessages =
    [
            "Scanning for devices...",
            "Disk Device(s) Found: SATA-1",
            "Starting disk devices...",
            "Mounting file systems...",
            "Checking disk integrity...",
            "Disk integrity OK!",
            "Configuring network interfaces...",
            "--- NETWORK BOOTING DISABLED!",
            "Scanning disks for bootable parts...",
            "OS found 1: SATA-1 PARTITION-1",
    ];

    private readonly string[] _afterBootMessages =
    [
        "Selected Boot Partition: SATA-1 PARTITION 1, Type: MBR",
        "Booting OS..."
    ];

    public BootPage()
    {
        InitializeComponent();

        _biosSettingsService = (Application.Current as App)?.Host?.Services.GetRequiredService<BIOSSettingsService>()
                               ?? throw new InvalidOperationException("BIOSSettingsService is not available");
        _firstBootOrder = _biosSettingsService.Settings.FirstBootOption;

        if (Window.Current?.CoreWindow != null)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        SetBootPartition();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(new Random().Next(200, 800))
        };
        _timer.Tick += OnTimerTick;
        _currentMessages = _postMessages;
        _timer.Start();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (Window.Current?.CoreWindow != null)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }
    }

    private void SetBootPartition()
    {
        switch (_firstBootOrder)
        {
            case "Simulated Operating System":
                _afterBootMessages[0] = "Selected Boot Partition: SATA-1 PARTITION 1, Type: MBR";
                break;
            case "Network Boot":
                _afterBootMessages[0] = "Selected Boot Partition: Network Boot, Type: PXE";
                break;
        }
    }

    private void OnTimerTick(object? sender, object? e)
    {
        if (_currentMessages == _afterBootMessages && isEnteringBIOS)
        {
            _messageIndex = _currentMessages.Length;
        }
        if (_messageIndex < _currentMessages.Length)
        {
            string message = _currentMessages[_messageIndex];

            if (message.Contains("---"))
            {
                ConsoleLogger.Log(message[4..], LogType.Error);
            }
            else if (message.Contains("***"))
            {
                ConsoleLogger.Log(message[4..], LogType.Info);
            }
            else
            {
                ConsoleLogger.Log(message, LogType.Info);
                BootInfoText.Text += message + "\n";
            }
            _messageIndex++;
            _timer.Interval = TimeSpan.FromMilliseconds(new Random().Next(200, 800));
        }
        else
        {
            if (_currentMessages == _postMessages)
            {
                _currentMessages = _beforeBootMessages;
                isBusy = false;
                SettingsInfoText.Visibility = Visibility.Visible;
                _firstBootOrder = _biosSettingsService.Settings.FirstBootOption;
                SetBootPartition();
            }
            else if (_currentMessages == _beforeBootMessages)
            {
                _currentMessages = _afterBootMessages;
                SettingsInfoText.Visibility = Visibility.Collapsed;
                isBusy = true;
            }
            else
            {
                _timer.Stop();
                if (!isEnteringBIOS)
                {
                    switch (_firstBootOrder)
                    {
                        case "Simulated Operating System":
                            Frame.Navigate(typeof(BootAnimationPage));
                            break;
                        case "Network Boot":
                            Frame.Navigate(typeof(NetworkBootPage));
                            break;

                        default:
                            break;

                    }
                }
            }
            _messageIndex = 0;
        }
    }

    public void ChangeSettingsInfoText()
    {
        isEnteringBIOS = true;
        ConsoleLogger.Log("Entering firmware settings...", LogType.Info);
        SettingsInfoText.Text = "Entering firmware settings...";
    }

    private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
    {
        if (Window.Current?.Content is Frame currentFrame)
        {
            if (args.VirtualKey == VirtualKey.F2)
            {
                if (!isBusy && !isEnteringBIOS)
                {
                    ChangeSettingsInfoText();
                    while (!isBusy)
                    {
                        await Task.Delay(100);
                    }
                    currentFrame.Navigate(typeof(BIOSInfoPage));
                    ConsoleLogger.Log("Entered BIOS Firmware Settings", LogType.Info);
                }
            }
        }
    }
}
