using Discord;
using Discord.WebSocket;
using PootchdayBot.Database;
using PootchdayBot.Database.Models;
using System.Text;
using System.Xml.Serialization;

namespace PootchdayBot.Services
{
    internal static class Xml
    {
        public static async Task<List<FileAttachment>> CreateXMLBackup(SocketGuild guild)
        {
            // Get DB Data
            var guildConfigList = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == guild.Id);
            var birthdayList = DatabaseContext.DB.Birthdays.Where(x => x.GuildID == guild.Id).ToList();

            // Create DiscordAPI Stream List
            List<FileAttachment> backupAttachments = new List<FileAttachment>();
            // Serialize as XML for GuildConfig
            XmlSerializer bw = new XmlSerializer(typeof(GuildConfig));

            // Serialize GuildConfig
            Stream streamConfig = new MemoryStream();
            bw.Serialize(streamConfig, guildConfigList);

            // add to Discord Stream List
            backupAttachments.Add(new FileAttachment(streamConfig, "BackupGuildConfig.xml"));

            // If Minimum one Birthday exist
            if (birthdayList.Count > 0)
            {
                // Serialize as XML for List<Birthdays>
                bw = new XmlSerializer(typeof(List<Birthdays>));

                // Serialize Birthdays
                Stream streamBirthday = new MemoryStream();
                
                bw.Serialize(streamBirthday, birthdayList);
                // add to Discord Stream List
                backupAttachments.Add(new FileAttachment(streamBirthday, "BackupBirthdays.xml"));
            }
            // Return Discord Stream List
            return backupAttachments;
        }
    }
}
