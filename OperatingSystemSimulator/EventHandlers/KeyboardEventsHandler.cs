
using Microsoft.UI.Windowing;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace OperatingSystemSimulator.EventHandlers;

public class KeyboardEventsHandler
{
    public class KeyboardEventHandler
    {
        private static KeyboardEventHandler? instance;
        private static readonly object lockObject = new();

        private App _app = (App)Application.Current;

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

        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!ProcessManager.Instance.IsTurnedOn) 
            {
                return;
            }
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Running);
            ProcessManager.Instance.InterruptQueueAsync(1);
            Frame currentFrame = (Frame)Window.Current!.Content!;

            if (args.VirtualKey == VirtualKey.F11)
            {
                switch (currentFrame?.Content)
                {
                    case DesktopPage:
                        if (Window.Current.CoreWindow!.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                        {
                            string[] parameters = { "PID: 1", "Process Name: Kernel", "MANUALY_TRIGGERED" };
                            ConsoleLogger.Log("Manual BughCheck Triggered", LogType.Info);
                            currentFrame.Navigate(typeof(BugCheckPage), parameters);
                            await Task.Delay(50);
                            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);
                        }
                        break;
                }
            }
            else if (args?.VirtualKey == VirtualKey.F6)
            {
                if (Window.Current.CoreWindow!.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                {
                    _app = (App)Application.Current;

                    if (_app.HardwareWindow == null || !_app.HardwareWindow.Visible)
                    {
                        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(750, 312);
                        _app.HardwareWindow = new Window
                        {
                            Title = "Virtual Hardware",
                            Content = new HardwarePage()
                        };
                        _app.HardwarePage = (HardwarePage)_app.HardwareWindow.Content;

                    }
                    _app.HardwareWindow.Activate();
                    var overlappedPresenter = (OverlappedPresenter)_app.HardwareWindow.AppWindow.Presenter;
                    overlappedPresenter.IsResizable = false;
                    ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1200, 720);
                }
            }
            else if (args?.VirtualKey == VirtualKey.F7)
            {
                if (Window.Current.CoreWindow!.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                {
                    _app = (App)Application.Current;

                    if (_app.PageListWindow == null || !_app.PageListWindow.Visible)
                    {
                        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(500, 500);
                        _app.PageListWindow = new Window
                        {
                            Title = "Page List",
                            Content = new PageListPage()
                        };
                        _app.PageListPage = (PageListPage)_app.PageListWindow.Content;

                    }
                    _app.PageListWindow.Activate();
                    var overlappedPresenter = (OverlappedPresenter)_app.PageListWindow.AppWindow.Presenter;
                    //overlappedPresenter.IsResizable = false;
                    ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1200, 720);
                }
            }
            else if (args?.VirtualKey == VirtualKey.Q)
            {
                if (currentFrame?.Content is DesktopPage)
                {
                    if (Window.Current!.CoreWindow!.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
                    {
                        await ProcessManager.Instance.TerminateFocusedProcess();
                    }
                }
            }
            await Task.Delay(1);
            HardwarePageViewModel.Instance.SetHardwareStatus(HardwareProperties.KeyStroke, HardwareStatuses.Idle);
        }
    }
}
