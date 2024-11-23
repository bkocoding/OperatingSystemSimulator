using Microsoft.UI.Xaml.Controls.Primitives;
using OperatingSystemSimulator.ProcessHelper;
using Windows.Foundation;
using Windows.UI.Core;

namespace OperatingSystemSimulator.EventHandlers
{
    public class MouseEventsHandler
    {
        private static MouseEventsHandler? instance;
        private static readonly object lockObject = new();

        private Point lastKnownPositionInsideWindow;
        private bool isOutsideWindow;
        private Popup? draggingPopup;
        private Point initialPointerOffset;

        public event Action<Point>? MouseMoved;

        public static MouseEventsHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new MouseEventsHandler();
                        }
                    }
                }
                return instance;
            }
        }

        private MouseEventsHandler()
        {
            Window.Current.CoreWindow.PointerMoved += OnPointerMoved;
            Window.Current.CoreWindow.PointerReleased += OnPointerReleased;
        }

        private void OnPointerMoved(CoreWindow sender, PointerEventArgs e)
        {
            var pointerPosition = e.CurrentPoint.Position;
            var windowBounds = Window.Current.Bounds;

            var constrainedX = Math.Max(0, Math.Min(pointerPosition.X, windowBounds.Width));
            var constrainedY = Math.Max(0, Math.Min(pointerPosition.Y, windowBounds.Height));

            if (draggingPopup != null)
            {
                var content = draggingPopup.Child as FrameworkElement;
                if (content != null)
                {
                    var popupWidth = content.ActualWidth > 0 ? content.ActualWidth : 100;
                    var popupHeight = content.ActualHeight > 0 ? content.ActualHeight : 100;

                    // Desktop Page taskbar stackpanel check
                    var maxHeight = windowBounds.Height - 55;

                    var newLeft = Math.Max(0, Math.Min(constrainedX - initialPointerOffset.X, windowBounds.Width - popupWidth));
                    var newTop = Math.Max(0, Math.Min(constrainedY - initialPointerOffset.Y, maxHeight - popupHeight));

                    draggingPopup.HorizontalOffset = newLeft;
                    draggingPopup.VerticalOffset = newTop;
                }
            }
        }

        private void OnPointerReleased(CoreWindow sender, PointerEventArgs e)
        {
            draggingPopup = null;
        }

        public void StartDragging(int pid, Point initialPointerPosition)
        {
//            ProcessManager.Instance.BringToFront(pid);
            draggingPopup = ProcessManager.Instance.GetProcessByPid(pid).Popup;
            initialPointerOffset = new Point(
                initialPointerPosition.X - draggingPopup.HorizontalOffset,
                initialPointerPosition.Y - draggingPopup.VerticalOffset);
        }

        public Point GetLastKnownPositionInsideWindow()
        {
            return lastKnownPositionInsideWindow;
        }

        public bool IsPointerOutsideWindow()
        {
            return isOutsideWindow;
        }
    }
}
