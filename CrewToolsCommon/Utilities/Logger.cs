namespace CrewToolsCommon.Utilities
{
    public class Logger
    {
        private const string _path = "log.txt";


        public static void Clean()
        {
            File.Delete(_path);
        }

        public static void Info(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.WriteLine(ParseMessage(message, color));
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine(ParseMessage($"ERROR: {message}", ConsoleColor.Red));
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.WriteLine(ParseMessage($"WARNING: {message}", ConsoleColor.Yellow));
            Console.ResetColor();
        }

        private static string ParseMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            DateTime time = DateTime.Now;
            string customTime = time.ToString("HH:mm:ss");

            message = $"[{customTime}] {message}";
            File.AppendAllLines(_path, [message]);
            return message;
        }
    }
}
