using Microsoft.UI.Xaml.Media.Animation;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;

namespace OperatingSystemSimulator.Pages;

public sealed partial class BootAnimationPage : Page
{
    public BootAnimationPage()
    {
        InitializeComponent();
        StartAnimationSequence();
    }

    private async void StartAnimationSequence()
    {
        await Task.Delay(3000);

        ProcessManager.Instance.StartOSServices();

        EmojiText.Visibility = Visibility.Visible;

        await Task.Delay(2000);
        Spinner.Visibility = Visibility.Visible;
        StartSpinnerAnimation();

        await Task.Delay(3000);
        ProcessManager.Instance.StartLogOnUser();
        NavigateToLogOn();
    }

    private void StartSpinnerAnimation()
    {
        RotateTransform rotateTransform = new();
        Spinner.RenderTransform = rotateTransform;
        Spinner.RenderTransformOrigin = new Point(0.5, 0.5);

        DoubleAnimation rotateAnimation = new()
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
            RepeatBehavior = RepeatBehavior.Forever
        };

        Storyboard storyboard = new();
        storyboard.Children.Add(rotateAnimation);
        Storyboard.SetTarget(rotateAnimation, rotateTransform);
        Storyboard.SetTargetProperty(rotateAnimation, "Angle");

        storyboard.Begin();
    }

    private static void NavigateToLogOn()
    {
        if (Window.Current?.Content is Frame currentFrame)
        {
            currentFrame.Navigate(typeof(WelcomePage));
        }
    }
}
