using System.ComponentModel.DataAnnotations.Schema;

namespace PootchdayBot.Database.Models
{
    internal class GuildConfig
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public ulong GuildID { get; set; }
        public ulong AnnounceChannelID { get; set; }
        public ulong ModRoleID { get; set; }
        public string? CustomMessage { get; set; }
        public bool Ping { get; set; }
    }
}
