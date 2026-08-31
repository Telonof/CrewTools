namespace CrewToolsCommon.Utilities
{
    public class Logger
    {
        private const string _path = "log.txt";
        private static readonly List<string> _logs = [];

        public static void Clean()
        {
            File.Delete(_path);
        }

        public static void WriteAndFlush()
        {
            File.WriteAllLines(_path, _logs);
            _logs.Clear();
        }

        public static void Info(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine(ParseMessage(message, color));
            Console.ResetColor();
        }

        public static void Error(string message, bool exitMessage = false)
        {
            Console.Error.WriteLine(ParseMessage($"ERROR: {message}", ConsoleColor.Red));
            Console.ResetColor();

            if (exitMessage)
                Logger.WriteAndFlush();
        }

        public static void Warning(string message)
        {
            Console.WriteLine(ParseMessage($"WARNING: {message}", ConsoleColor.Yellow));
            Console.ResetColor();
        }
        
        public static void Banner(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void BannerHighlight(string message, string name)
        {
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($" {name}");
            Console.ResetColor();
        }

        private static string ParseMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            DateTime time = DateTime.Now;
            string customTime = time.ToString("HH:mm:ss");

            message = $"[{customTime}] {message}";
            _logs.Add(message);
            return message;
        }
    }
}
