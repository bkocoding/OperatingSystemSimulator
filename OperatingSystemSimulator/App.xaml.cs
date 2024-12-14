using System.Drawing;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using OperatingSystemSimulator.Services;
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
        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1280, 720);
    }

    protected Window? MainWindow { get; private set; }
    public HardwarePage HardwarePage { get; set; } = new HardwarePage();
    public IHost? Host { get; private set; }
    public Window HardwareWindow { get; set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var keyboardEventHandler = KeyboardEventHandler.Instance;
        var builder = this.CreateBuilder(args)
#if MAUI_EMBEDDING
            .UseMauiEmbedding<MauiControls.App>(maui => maui
                .UseMauiControls())
#endif
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
                        .AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.None);

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

        MainWindow.Activate();

        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(750, 260);
        HardwareWindow = new Window
        {
            Title = "Virtual Hardware",
            Content = HardwarePage,
        };

        var OPHardwareWindow = (OverlappedPresenter)HardwareWindow.AppWindow.Presenter;

        OPHardwareWindow.IsResizable = false;

        MainWindow.Closed += (s, e) => HardwareWindow.Close();
        HardwareWindow.Activate();
        ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1280, 720);
    }

}
