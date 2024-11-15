namespace OperatingSystemSimulator.Extras.ConsoleLogger
{    
        public static class ConsoleLogger
        {
            public static void Log(string message, LogType logtype)
            {
                switch (logtype)
                {
                    case LogType.Info:
                        Console.WriteLine($"[INFO] {message}");
                        break;

                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] {message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case LogType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[WARNING] {message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case LogType.Init:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[INIT] {message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"[UNKNOWN]: {message}");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                }


            }
        }
}
