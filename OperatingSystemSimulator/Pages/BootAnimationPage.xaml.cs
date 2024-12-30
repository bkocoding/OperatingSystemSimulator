using Microsoft.UI.Xaml.Media.Animation;
using OperatingSystemSimulator.FileHelper;
using OperatingSystemSimulator.ProcessHelper;
using OperatingSystemSimulator.Services;
using Windows.Foundation;

namespace OperatingSystemSimulator.Pages;

public sealed partial class BootAnimationPage : Page
{
    private BIOSSettingsService _biosSettingsService = (Application.Current as App)!.Host!.Services.GetRequiredService<BIOSSettingsService>()!;

    public BootAnimationPage()
    {
        InitializeComponent();
        HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.HdRead, HardwareStatuses.Running);
        StartAnimationSequence();
    }

    private async void StartAnimationSequence()
    {
        ProcessManager.Instance.StartOSServices();

        await Task.Delay(3000);

        EmojiText.Visibility = Visibility.Visible;

        await Task.Delay(2000);
        Spinner.Visibility = Visibility.Visible;
        StartSpinnerAnimation();

        await Task.Delay(3000);

        if (!BKOFSManager.Instance.ValidateOS())
        {
            BootAnimationText.Visibility = Visibility.Visible;
            _biosSettingsService.SaveLastBootState(false);
            await Task.Delay(3000);
        }
        else if (!_biosSettingsService.Settings!.WasLastBootSuccesful) 
        {
            BootAnimationText.Visibility = Visibility.Visible;
            await Task.Delay(3000);
        }
        else
        {
            ProcessManager.Instance.StartLogOnUser();
        }
        NavigateToNext();
    }

    private void StartSpinnerAnimation()
    {
        RotateTransform rotateTransform = new RotateTransform();
        Spinner.RenderTransform = rotateTransform;
        Spinner.RenderTransformOrigin = new Point(0.5, 0.5);

        DoubleAnimation rotateAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
            RepeatBehavior = RepeatBehavior.Forever
        };

        Storyboard storyboard = new Storyboard();
        storyboard.Children.Add(rotateAnimation);
        Storyboard.SetTarget(rotateAnimation, rotateTransform);
        Storyboard.SetTargetProperty(rotateAnimation, "Angle");

        storyboard.Begin();
    }

    private void NavigateToNext()
    {
        if (!_biosSettingsService.Settings!.WasLastBootSuccesful)
        {
            HardwarePageViewModel.Instance.SetHDOperation(HDOperations.Recovery);
            HardwarePageViewModel.Instance.SetRunningProcess("Recovery");
            Frame.Navigate(typeof(RecoveryPage));
        }
        else
        {
            Frame currentFrame = (Frame)Window.Current!.Content!;
            currentFrame?.Navigate(typeof(WelcomePage));
        }

    }
}
