namespace OperatingSystemSimulator.Extras.ConsoleLogger;

public static class ConsoleLogger
{
    public static void Log(string message, LogType logtype)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        switch (logtype)
        {
            case LogType.Info:
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[INFO] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.Init:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INIT] {message}");
                Console.ForegroundColor = oldColor;
                break;

            default:
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[UNKNOWN]: {message}");
                Console.ForegroundColor = oldColor;
                break;

        }


    }
}
