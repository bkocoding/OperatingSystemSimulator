using System.ComponentModel;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.ProcessHelper;

namespace OperatingSystemSimulator.NetworkHelper;
public class NetworkManager : INotifyPropertyChanged
{
    private static NetworkManager? instance;
    private static readonly object lockObject = new();

    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new NetworkManager();
                    }
                }
            }
            return instance;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _isConnected = false;
    public bool IsConnected
    {
        get { return _isConnected; }
        private set
        {
            _isConnected = value;
            OnNetworkStatusChanged();
        }
    }

    public async Task ConnectAsync()
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(3);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Running);
        ConsoleLogger.Log("Preparing to establish the network connection...", LogType.NetworkOutput);
        await Task.Delay(150);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Waiting);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Running);
        ConsoleLogger.Log("Initial handshake in progress...", LogType.NetworkInput);
        await ProcessManager.Instance.InterruptQueueAsync(3);
        await Task.Delay(150);

        await ProcessManager.Instance.EnqueueRunningProcessAsync(3);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
        await Task.Delay(10);

        ConsoleLogger.Log("Connection setup completed, network status: Online.", LogType.Info);
        IsConnected = true;
    }

    public async void Disconnect()
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(3);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Running);
        ConsoleLogger.Log("Preparing to terminate the network connection...", LogType.NetworkOutput);
        await Task.Delay(100);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Waiting);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Running);
        ConsoleLogger.Log("Disconnect handshake in progress...", LogType.NetworkInput);
        await ProcessManager.Instance.InterruptQueueAsync(3);
        await Task.Delay(100);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
        await Task.Delay(10);

        ConsoleLogger.Log("Disconnection completed, network status: Offline.", LogType.Info);
        IsConnected = false;
    }

    public static async Task DNSQueryAsync()
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(3);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Running);
        ConsoleLogger.Log("Preparing DNS query...",LogType.NetworkOutput);
        await Task.Delay(100);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Waiting);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Running);
        ConsoleLogger.Log("DNS Server handshake in progress...", LogType.NetworkInput);
        await ProcessManager.Instance.InterruptQueueAsync(3);
        await Task.Delay(150);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
        await Task.Delay(10);

    }

    public static async Task DownloadingWebSiteAsync(string url)
    {
        await ProcessManager.Instance.EnqueueRunningProcessAsync(3);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Running);
        ConsoleLogger.Log($"Starting download from {url}...", LogType.NetworkOutput);
        await Task.Delay(200);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Waiting);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Running);
        ConsoleLogger.Log("Receiving website data...", LogType.NetworkInput);
        await ProcessManager.Instance.InterruptQueueAsync(3);
        await Task.Delay(300);

        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
        await Task.Delay(10);

        ConsoleLogger.Log($"Website downloaded successfully from {url}.", LogType.Info);
    }

    public void DisconnectForcefully() 
    {
        IsConnected = false;
        var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
        dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkInput, HardwareStatuses.Idle);
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.NetworkOutput, HardwareStatuses.Idle);
        }).AsTask().Wait();
    }

    public void OnNetworkStatusChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
    }
}
