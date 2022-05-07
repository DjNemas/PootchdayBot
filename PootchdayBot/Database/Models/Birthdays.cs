using System.ComponentModel.DataAnnotations.Schema;

namespace PootchdayBot.Database.Models
{
    internal class Birthdays
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public ulong AccountID { get; set; }
        public ulong GuildID { get; set; }
        public string GlobalUsername { get; set; }
        public DateTime Birthday { get; set; }

        public Birthdays(ulong accountID, ulong guildID, string globalUsername, DateTime birthday)
        {
            AccountID = accountID;
            GuildID = guildID;
            GlobalUsername = globalUsername;
            Birthday = birthday;
        }
    }
}
