using PootchdayBot.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace PootchdayBot.Database.Models
{
    [XmlRoot("Birthdays")]
    public class Birthdays
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [XmlElement(ElementName = "ID")]
        public int ID { get; set; }

        [XmlElement(ElementName = "AccountID")]
        public ulong AccountID { get; set; }

        [XmlElement(ElementName = "GuildID")]
        public ulong GuildID { get; set; }

        [XmlElement(ElementName = "GlobalUsername")]
        public string GlobalUsername { get; set; }

        [XmlElement(ElementName = "Birthday")]
        public DateTime Birthday { get; set; }


        // For XML Serialisation
        private Birthdays() { }

        public Birthdays(ulong accountID, ulong guildID, string globalUsername, DateTime birthday)
        {
            AccountID = ValidChecker.IsZero(accountID);
            GuildID = ValidChecker.IsZero(guildID);
            GlobalUsername = ValidChecker.IsEmpty(globalUsername);
            Birthday = ValidChecker.IsNull(birthday);
        }
    }
}
