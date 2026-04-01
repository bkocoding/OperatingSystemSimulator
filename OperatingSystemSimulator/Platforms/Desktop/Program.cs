using System.Runtime.InteropServices;
using Uno.UI.Runtime.Skia;

namespace OperatingSystemSimulator;
public class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    [STAThread]
    public static void Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            AllocConsole();
        }

        var host = SkiaHostBuilder.Create()
            .App(() => new App())
            .UseX11()
            .UseLinuxFrameBuffer()
            .UseMacOS()
            .UseWindows()
            .Build();

        host.Run();
    }
}
