using Microsoft.UI.Windowing;
using Uno.Resizetizer;
using Windows.UI.ViewManagement;

namespace OperatingSystemSimulator;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1200, 720);
    }

    protected Window? MainWindow { get; private set; }
    public HardwarePage HardwarePage { get; set; }
    public PageListPage PageListPage { get; set; }
    public Window HardwareWindow { get; set; }
    public Window PageListWindow { get; set; }
    public IHost? Host { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var keyboardEventHandler = KeyboardEventHandler.Instance;
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);
                    logBuilder
                        .AddFilter("Windows.UI.Input.Preview.Injection.IInputInjectorTarget", LogLevel.None)
                        .AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.None)
                        .AddFilter("Microsoft.UI.Input.GestureRecognizer", LogLevel.None); //I still have no idea why gesture recogniser is hanging...

                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                }, enableUnoLogging: true)
                .UseSerilog(consoleLoggingEnabled: false, fileLoggingEnabled: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<BIOSSettingsService>();
                })
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.EnableHotReload();

#endif
        MainWindow.SetWindowIcon();

        Host = builder.Build();

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (MainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            MainWindow.Content = rootFrame;
        }

        if (rootFrame.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(BootPage), args.Arguments);
        }

        MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 1200, Height = 720 });
        MainWindow.Activate();

        HardwareWindow = new Window
        {
            Title = "Virtual Hardware",
            Content = new HardwarePage()
        };

        HardwarePage = (HardwarePage)HardwareWindow.Content;

        var OPHardwareWindow = (OverlappedPresenter)HardwareWindow.AppWindow.Presenter;

        OPHardwareWindow.IsResizable = false;
        OPHardwareWindow.IsMaximizable = false;

        HardwareWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 750, Height = OperatingSystem.IsWindows() ? 312 : 270 });
        HardwareWindow.Activate();

        PageListWindow = new Window
        {
            Title = "Page List",
            Content = new PageListPage()
        };
        PageListPage = (PageListPage)PageListWindow.Content;

        var OPPageListWindow = (OverlappedPresenter)PageListWindow.AppWindow.Presenter;

        //OPPageListWindow.IsResizable = false;
        PageListWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 500, Height = 500 });
        PageListWindow.Activate();

        MainWindow.Closed += (s, e) =>
        {
            HardwareWindow.Close();
            PageListWindow.Close();
        };
    }

}
