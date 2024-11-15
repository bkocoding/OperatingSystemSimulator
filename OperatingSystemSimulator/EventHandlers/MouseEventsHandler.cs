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
        }

        private void OnPointerMoved(CoreWindow sender, PointerEventArgs e)
        {
            var pointerPosition = e.CurrentPoint.Position;
            var windowBounds = Window.Current.Bounds;

            if (pointerPosition.X < 0 || pointerPosition.Y < 0 ||
                pointerPosition.X > windowBounds.Width || pointerPosition.Y > windowBounds.Height)
            {
                if (!isOutsideWindow)
                {
                    isOutsideWindow = true;
                    lastKnownPositionInsideWindow = new Point(pointerPosition.X, pointerPosition.Y);
                }
            }
            else
            {
                isOutsideWindow = false;
                lastKnownPositionInsideWindow = pointerPosition;

                MouseMoved?.Invoke(lastKnownPositionInsideWindow);
            }
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
