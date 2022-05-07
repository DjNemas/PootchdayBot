using PootchdayBot.Logging;
using System.Xml;
using PootchdayBot.Database;

namespace PootchdayBot.FolderManagment
{
    internal static class FolderFile
    {
        public static void InitAll()
        {
            InitFolder();
            CreateConfigFile();
            CreateDB();
        }

        private static void CreateDB()
        {
            DatabaseContext.DB.Database.EnsureCreated();
            Log.DebugDiscord("DB Created or Loaded");
        }

        private static void InitFolder()
        {
            CreateFolder(Folder.Log);
            CreateFolder(Folder.Config);
            CreateFolder(Folder.Database);
        }

        private static void CreateFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Console.WriteLine("Folder: " + folder + " created!");
            }
        }

        private static void CreateConfigFile()
        {
            if (!File.Exists(Path.Combine(Folder.Config, Files.Config)))
            {
                XmlDocument doc = new XmlDocument();
                XmlElement tokenElement = doc.CreateElement("token");
                tokenElement.InnerText = "Input Token Here!";
                doc.AppendChild(tokenElement);

                doc.Save(Path.Combine(Folder.Config, Files.Config));

                Log.DebugDiscord("Config File Created!");
            }
        }

        public static string GetToken()
        {
            XmlDocument tokenElement = new XmlDocument();
            tokenElement.Load(Path.Combine(Folder.Config, Files.Config));
            return tokenElement.InnerText;
        }

    }
}
