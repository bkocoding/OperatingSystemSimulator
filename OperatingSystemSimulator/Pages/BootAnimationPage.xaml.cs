using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace OperatingSystemSimulator.Pages
{
    public sealed partial class BootAnimationPage : Page
    {
        public BootAnimationPage()
        {
            this.InitializeComponent();
            StartAnimationSequence();
        }

        private async void StartAnimationSequence()
        {
            await Task.Delay(3000);

            EmojiText.Visibility = Visibility.Visible;

            await Task.Delay(2000);

            Spinner.Visibility = Visibility.Visible;
            StartSpinnerAnimation();

            await Task.Delay(3000);
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
