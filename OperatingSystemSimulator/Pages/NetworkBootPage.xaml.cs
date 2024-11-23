using OperatingSystemSimulator.Extras.ConsoleLogger;
using Windows.System;
using Windows.UI.Core;

namespace OperatingSystemSimulator.Pages;
public sealed partial class NetworkBootPage : Page
{
    public NetworkBootPage()
    {
        InitializeComponent();
        if (Window.Current?.CoreWindow != null)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }
        ConsoleLogger.Log("No IPv4 address found to try to PXE boot!", LogType.Error);
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

    private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
    {
        if (Window.Current?.Content is Frame currentFrame)
        {
            if (args.VirtualKey == VirtualKey.F2)
            {
                currentFrame.Navigate(typeof(BIOSInfoPage));
                ConsoleLogger.Log("Entered BIOS Firmware Settings", LogType.Info);
            }
        }
    }
}
