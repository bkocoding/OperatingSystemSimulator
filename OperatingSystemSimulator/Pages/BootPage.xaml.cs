using OperatingSystemSimulator.EventHandlers;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.Services;
using OperatingSystemSimulator.ViewModels.PageViewModels;
using Windows.System;
using Windows.UI.Core;
using MemoryManager = OperatingSystemSimulator.MemoryHelper.MemoryManager;

namespace OperatingSystemSimulator.Pages;
public sealed partial class BootPage : Page
{
    private DispatcherTimer _timer;
    public bool isBusy = true;
    private bool isEnteringBIOS = false;
    private int _messageIndex = 0;
    private string[] _currentMessages;
    private string _firstBootOrder;
    private readonly HardwarePageViewModel HardwarePageViewModel = HardwarePageViewModel.Instance;

    private readonly BIOSSettingsService _biosSettingsService;

    private readonly string[] _postMessages =
    {
            $"RAM OK! Size: {MemoryManager.memorySize} Byte(s)",
    };

    private readonly string[] _beforeBootMessages =
    {
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
    };

    private readonly string[] _afterBootMessages =
    {
        "Selected Boot Partition: SATA-1 PARTITION 1, Type: MBR",
        "Booting OS..."
    };

    public BootPage()
    {
        InitializeComponent();
        MouseEventsHandler.Instance.Initialize();
        _biosSettingsService = (Application.Current as App)?.Host?.Services.GetRequiredService<BIOSSettingsService>()!;
        _firstBootOrder = _biosSettingsService.Settings.FirstBootOption;

        Window.Current!.CoreWindow!.KeyDown += CoreWindow_KeyDown;

        SetBootPartition();
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(new Random().Next(200, 800));
        _timer.Tick += OnTimerTick!;
        _currentMessages = _postMessages;
        StartPOST();
    }

    private async void StartPOST() {

       await MemoryManager.Instance.TestMemory();
        _timer.Start();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Window.Current!.CoreWindow!.KeyDown -= CoreWindow_KeyDown;
    }

    private void SetBootPartition()
    {
        HardwarePageViewModel.SetRunningProcess("BIOS Firmware");
        HardwarePageViewModel.SetHDOperation(HDOperations.NotMounted);
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

    private void OnTimerTick(object sender, object e)
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
                ConsoleLogger.Log(message.Substring(4), LogType.Error);
            }
            else if (message.Contains("***"))
            {
                ConsoleLogger.Log(message.Substring(4), LogType.Info);
            }
            else
            {
                if (message == "Scanning for devices..." ||
                message == "Disk Device(s) Found: SATA-1" ||
                message == "Starting disk devices..." ||
                message == "Mounting file systems..." ||
                message == "Checking disk integrity..." ||
                message == "Disk integrity OK!")
                {
                    HardwarePageViewModel.SetHDOperation(HDOperations.SMART);
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
                }
                else if (message == "Configuring network interfaces...")
                {
                    HardwarePageViewModel.SetHDOperation(HDOperations.NotMounted);
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);

                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Running);
                    Task.Delay(100).Wait();
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);


                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Running);
                    Task.Delay(100).Wait();
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);



                }
                else if (message == "Scanning disks for bootable parts...")
                {
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
                    HardwarePageViewModel.SetHDOperation(HDOperations.MBR);
                }
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
                _firstBootOrder = _biosSettingsService.Settings!.FirstBootOption;
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
                    HardwarePageViewModel.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Idle);
                    switch (_firstBootOrder)
                    {
                        case "Simulated Operating System":
                            HardwarePageViewModel.SetHDOperation(HDOperations.OperatingSystem);
                            HardwarePageViewModel.SetRunningProcess("Boot Manager");
                            Frame.Navigate(typeof(BootAnimationPage));
                            break;
                        case "Network Boot":
                            HardwarePageViewModel.SetHDOperation(HDOperations.NotMounted);
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
        Frame currentFrame = (Frame)Window.Current!.Content!;

        if (args.VirtualKey == VirtualKey.F2)
        {

            if (!isBusy && !isEnteringBIOS)
            {
                ChangeSettingsInfoText();
                while (!isBusy)
                {
                    await Task.Delay(100);
                }
                HardwarePageViewModel.SetHDOperation(HDOperations.NotMounted);
                currentFrame.Navigate(typeof(BIOSInfoPage));
                ConsoleLogger.Log("Entered BIOS Firmware Settings", LogType.Info);
            }


        }


    }
}
