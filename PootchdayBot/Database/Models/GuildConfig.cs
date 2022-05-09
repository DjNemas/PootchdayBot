using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace PootchdayBot.Database.Models
{
    [XmlRoot("GuildConfig")]
    public class GuildConfig
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [XmlElement(ElementName = "ID")]
        public int ID { get; set; }

        [XmlElement(ElementName = "GuildID")]
        public ulong GuildID { get; set; }

        [XmlElement(ElementName = "AnnounceChannelID")]
        public ulong AnnounceChannelID { get; set; }

        [XmlElement(ElementName = "ModRoleID")]
        public ulong ModRoleID { get; set; }

        [XmlElement(ElementName = "BirthdayRoleID")]
        public ulong BirthdayRoleID { get; set; }

        [XmlElement(ElementName = "CustomMessage")]
        public string? CustomMessage { get; set; }

        [XmlElement(ElementName = "Ping")]
        public bool Ping { get; set; }

        // For XML Serialisation
        private GuildConfig() { }

        public GuildConfig(ulong guildID, ulong announceChannelID, ulong modRoleID = 0, ulong birthdayRoleID = 0, string customMessage = "", bool ping = true)
        {
            GuildID = guildID;
            AnnounceChannelID = announceChannelID;
            ModRoleID = modRoleID;
            BirthdayRoleID = birthdayRoleID;
            CustomMessage = customMessage;
            Ping = ping;
        }
    }
}
