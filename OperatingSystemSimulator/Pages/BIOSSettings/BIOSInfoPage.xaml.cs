using OperatingSystemSimulator.Extras.ConsoleLogger;
using Windows.System;
using Windows.UI.Core;

namespace OperatingSystemSimulator.Pages.BIOSSettings;

public sealed partial class BIOSInfoPage : Page
	{
    public BIOSInfoViewModel ViewModel { get; set; }
    public BIOSInfoPage()
		{
			InitializeComponent();
        ViewModel = new BIOSInfoViewModel();
        DataContext = ViewModel;
        Window.Current!.CoreWindow!.KeyDown += CoreWindow_KeyDown;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.StartTimer();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Window.Current!.CoreWindow!.KeyDown -= CoreWindow_KeyDown;
    }

    private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args) 
    {
        Frame currentFrame = (Frame)Window.Current!.Content!;

        if (args.VirtualKey == VirtualKey.Right) 
        {
            ViewModel.StopTimer();
            currentFrame.Navigate(typeof(BIOSBootPage));
        }

        if (args.VirtualKey == VirtualKey.F8) 
        {
            currentFrame?.Navigate(typeof(BootPage));

            ConsoleLogger.Log("Discarding changes, rebooting...", LogType.Info);
        }

        if (args.VirtualKey == VirtualKey.F9) 
        {
            currentFrame?.Navigate(typeof(BootPage));
            ConsoleLogger.Log("Saving changes, rebooting...", LogType.Info);
        }
        
    }
}
