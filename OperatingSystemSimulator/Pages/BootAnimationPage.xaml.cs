using Microsoft.UI.Xaml.Media.Animation;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;

namespace OperatingSystemSimulator.Pages
{
    public sealed partial class BootAnimationPage : Page
    {
        public BootAnimationPage()
        {
            InitializeComponent();
            HardwarePageViewModel.Instance.HdRead = new BrushConverter().ConvertFromString("#00FF00") as SolidColorBrush;
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
            ProcessManager.Instance.StartLogOnUser();
            NavigateToLogOn();
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

        private void NavigateToLogOn() 
        {
            Frame currentFrame = (Frame)Window.Current.Content;
            currentFrame?.Navigate(typeof(WelcomePage));
        }
    }
}
