using Microsoft.UI.Xaml.Controls.Primitives;

namespace OperatingSystemSimulator.ProcessHelper
{
    public class ProcessBlock
    {
        private readonly Random random = new Random();
        private double previousWidthOffset = 200;
        private double previousHeightOffset = 200;

        public int Pid { get; }
        public Popup? Popup { get; set; }
        public object? App { get; }
        public string Name { get; }
        public int StartAdress { get; set; }
        public int EndAdress { get; set; }
        public bool IsIdle { get; set; } = false;
        public int Size { get; set; }

        public ProcessBlock(int pid, Popup popup, object app, string name)
        {
            Pid = pid;
            Popup = popup;
            App = app;
            Name = name;
            InitializePopup();
        }

        private void InitializePopup()
        {
            Popup.Child = App as UIElement;

            double newWidthOffset;
            double newHeightOffset;

            do
            {
                newWidthOffset = random.Next(50, 601);
            } while (newWidthOffset == previousWidthOffset);

            do
            {
                newHeightOffset = random.Next(50, 601);
            } while (newHeightOffset == previousHeightOffset);

            previousWidthOffset = newWidthOffset;
            previousHeightOffset = newHeightOffset;

            Popup.HorizontalOffset = (Window.Current.Bounds.Width - newWidthOffset) / 2;
            Popup.VerticalOffset = (Window.Current.Bounds.Height - newHeightOffset) / 2;
            Popup.IsOpen = true;
        }
    }
}
