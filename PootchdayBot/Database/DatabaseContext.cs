using Microsoft.EntityFrameworkCore;
using PootchdayBot.Database.Models;
using PootchdayBot.FolderManagment;

namespace PootchdayBot.Database
{
    internal class DatabaseContext : DbContext
    {
        public readonly static DatabaseContext DB = new DatabaseContext();
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<Birthdays> Birthdays { get; set; }

        public string DbPath = Path.Combine(Folder.Database, Files.Database);

        protected override void OnConfiguring(DbContextOptionsBuilder options) 
            => options.UseSqlite($"Data Source={DbPath}");

    }
}
