
using OperatingSystemSimulator.Extras.ConsoleLogger;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace OperatingSystemSimulator.EventHandlers

{
    public class KeyboardEventsHandler
    {
        public class KeyboardEventHandler
        {
            private static KeyboardEventHandler? instance;
            private static readonly object lockObject = new();

            private App _app = (App)Application.Current;

            private bool isEnteringFirmwareSettings = false;
            public static KeyboardEventHandler Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (lockObject)
                        {
                            if (instance == null)
                            {
                                instance = new KeyboardEventHandler();
                            }
                        }
                    }
                    return instance;
                }
            }

            private KeyboardEventHandler()
            {
                if (Window.Current != null && Window.Current.CoreWindow != null)
                {
                    Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
                }
            }

            private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
            {
                Frame currentFrame = (Frame)Window.Current.Content;

                if (args.VirtualKey == VirtualKey.F11)
                {

                    switch (currentFrame?.Content)
                    {
                        case DesktopPage:
                            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                            {
                                string[] parameters = { "PID: 1\nProcess Name: OS", "MANUALY_TRIGGERED" };
                                ConsoleLogger.Log("Manual BughCheck Triggered", LogType.Info);
                                Task.Delay(1000).Wait();
                                currentFrame.Navigate(typeof(BugCheckPage), parameters);
                            }
                            break;
                    }

                }
                else if (args?.VirtualKey == VirtualKey.F6)
                {
                    if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                    {
                        _app = (App)Application.Current;

                        if (_app.HardwareWindow == null || !_app.HardwareWindow.Visible)
                        {
                            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1280, 260);
                            _app.HardwareWindow = new Window
                            {
                                Title = "Virtual Hardware",
                                Content = _app.HardwarePage
                            };
                        }
                        _app.HardwareWindow.Activate();
                        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1280, 720);
                    }

                }

            }
        }
    }
}
