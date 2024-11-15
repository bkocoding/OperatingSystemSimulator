
using Microsoft.UI.Xaml.Input;
using OperatingSystemSimulator.Extras.ConsoleLogger;
using OperatingSystemSimulator.Services;
using Windows.System;
using Windows.UI.Core;

namespace OperatingSystemSimulator.Pages.BIOSSettings
{
    public sealed partial class BIOSBootPage : Page
    {
        public BIOSBootViewModel ViewModel { get; set; }
        private int HighLighted = 0;
        private readonly BIOSSettingsService _biosSettingsService;

        public BIOSBootPage()
        {
            InitializeComponent();
            ViewModel = new BIOSBootViewModel();
            DataContext = ViewModel;
            _biosSettingsService = (Application.Current as App)?.Host?.Services.GetRequiredService<BIOSSettingsService>();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
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

        private void HighlightUp()
        {
            if (HighLighted == 1)
            {
                FBOBorder.BorderThickness = new Thickness(1);
                SBOBorder.BorderThickness = new Thickness(0);
                HighLighted = 0;
            }

        }

        private void HighlightDown()
        {
            if (HighLighted == 0)
            {
                FBOBorder.BorderThickness = new Thickness(0);
                SBOBorder.BorderThickness = new Thickness(1);
                HighLighted = 1;
            }

        }

        private void ValueChange()
        {
            string old = SBOText.Text;
            SBOText.Text = FBOText.Text;
            FBOText.Text = old;
        }

        private void SaveValues()
        {

            var settings = new Models.BIOSSettings() { FirstBootOption = FBOText.Text, SecondBootOption = SBOText.Text };
            _biosSettingsService.Settings = settings;
            _biosSettingsService.SaveSettings();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Frame currentFrame = (Frame)Window.Current.Content;

            if (args.VirtualKey == VirtualKey.Left)
            {
                currentFrame.Navigate(typeof(BIOSInfoPage));
            }

            if (args.VirtualKey == VirtualKey.Up)
            {
                HighlightUp();
            }

            if (args.VirtualKey == VirtualKey.Down)
            {
                HighlightDown();
            }

            if (args.VirtualKey == VirtualKey.F5)
            {
                ValueChange();
            }

            if (args.VirtualKey == VirtualKey.F6)
            {
                ValueChange();
            }

            if (args.VirtualKey == VirtualKey.F8)
            {
                currentFrame?.Navigate(typeof(BootPage));
                currentFrame.BackStack.Clear();

                ConsoleLogger.Log("Discarding changes, rebooting...", LogType.Info);
            }

            if (args.VirtualKey == VirtualKey.F9)
            {
                SaveValues();
                currentFrame?.Navigate(typeof(BootPage));
                currentFrame.BackStack.Clear();
                ConsoleLogger.Log("Saving changes, rebooting...", LogType.Info);
            }


        }
    }
}
