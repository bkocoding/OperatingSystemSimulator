using Windows.System;
using Windows.UI.Core;

namespace OperatingSystemSimulator.Pages;
public sealed partial class NetworkBootPage : Page
{
    public NetworkBootPage()
    {
        InitializeComponent();
        Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        ConsoleLogger.Log("No IPv4 adress found to try to PXE boot!", LogType.Error);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }
    
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
    }

    private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args) 
    {
        Frame currentFrame = (Frame)Window.Current.Content;

        if (args.VirtualKey == VirtualKey.F2)
        {
            currentFrame.Navigate(typeof(BIOSInfoPage));
            ConsoleLogger.Log("Entered BIOS Firmware Settings", LogType.Info);
        }
    }
}
