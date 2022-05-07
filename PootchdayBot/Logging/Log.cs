using PootchdayBot.FolderManagment;

namespace PootchdayBot.Logging
{
    internal static class Log
    {
        private static readonly string DateFormat = "dd.MM.yyyy HH:mm";

        public static void DebugDiscord(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Discord] [DEBUG] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log) , debugMessage);
            if (console)
                Console.Write(debugMessage);
        }

        public static void ErrorDiscord(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Discord] [ERROR] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log), debugMessage);
            if (console)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(debugMessage);
                Console.ResetColor();
            }
        }

        public static void DebugInteraction(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Interaction] [DEBUG] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log), debugMessage);
            if (console)
                Console.Write(debugMessage);
        }

        public static void ErrorInteraction(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Interactions] [ERROR] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log), debugMessage);
            if (console)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(debugMessage);
                Console.ResetColor();
            }
        }

        public static void DebugDatabase(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Database] [DEBUG] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log), debugMessage);
            if (console)
                Console.Write(debugMessage);
        }

        public static void ErrorDatabase(string message, bool console = true)
        {
            string debugMessage = DateTime.Now.ToString(DateFormat) + " [Database] [ERROR] " + message + "\n";
            File.AppendAllText(Path.Combine(Folder.Log, Files.Log), debugMessage);
            if (console)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(debugMessage);
                Console.ResetColor();
            }
        }
    }
}
