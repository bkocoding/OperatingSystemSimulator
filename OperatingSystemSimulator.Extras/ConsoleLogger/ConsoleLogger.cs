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

            case LogType.FileDialog:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[FILE DIALOG] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.MessageBox:
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[MESSAGE] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.Result:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[RESULT] {message}");
                Console.ForegroundColor = oldColor;
                break;

            case LogType.Interrupt:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[INTERRUPT] {message}");
                Console.ForegroundColor = oldColor;
                break;
            
            case LogType.Queue:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[QUEUE] {message}");
                Console.ForegroundColor = oldColor;
                break;

            default:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[UNKNOWN]: {message}");
                Console.ForegroundColor = oldColor;
                break;

        }


    }
}
