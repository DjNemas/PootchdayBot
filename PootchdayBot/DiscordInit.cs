using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PootchdayBot.BirthdayFunctions;
using PootchdayBot.Database;
using PootchdayBot.Database.Models;
using PootchdayBot.Logging;
using PootchdayBot.Services;

namespace PootchdayBot
{
    internal class DiscordInit
    {
        private DiscordSocketClient client;

        private InteractionService interactionService;

        private Interaction interaction;

        private BirthdayReminder birthdayReminder;

        private IServiceProvider serviceProvider;

        private bool deleteAllGuildSlashCommands = false;

        public DiscordInit()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.All
            });

            interactionService = new InteractionService(client, new InteractionServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = RunMode.Async
            });

            serviceProvider = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(interactionService)
                .AddSingleton<Interaction>()
                .AddSingleton<DatabaseContext>()
                .AddSingleton<BirthdayReminder>()
                .BuildServiceProvider();
        }

        public async Task MainAsync()
        {

            client.Log += APILog;
            client.Connected += Client_Connected;
            client.GuildAvailable += Client_GuildAvailable;
            client.RoleDeleted += Client_RoleDeleted;
            client.UserLeft += Client_UserLeft;

            await client.SetGameAsync("/hilfe", type: ActivityType.Listening);

            interaction = serviceProvider.GetRequiredService<Interaction>();
            await interaction.InstallInteractionServiceAsync();

            birthdayReminder = serviceProvider.GetRequiredService<BirthdayReminder>();
            birthdayReminder.StartReminder();

            await client.LoginAsync(TokenType.Bot, FolderManagment.FolderFile.GetToken());
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private async Task Client_UserLeft(SocketGuild guild, SocketUser user)
        {
            Birthdays birthdayUser = DatabaseContext.DB.Birthdays.FirstOrDefault(x => x.GuildID == guild.Id && x.AccountID == user.Id);

            if (birthdayUser == null)
                return;

            DatabaseContext.DB.Birthdays.Remove(birthdayUser);
            Log.DebugDatabase("User: " + user.Username + " ID: " + user.Id + " left the Guild.\n" +
                "User removed from Database");
        }

        private async Task Client_RoleDeleted(SocketRole role)
        {
            // Reset Birthday or Modrole in GuildConfig if role will be deleted
            GuildConfig gConfig = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == role.Guild.Id);
            // ModRole
            if (role.Id == gConfig.ModRoleID)
            {
                gConfig.ModRoleID = 0;
                await DatabaseContext.DB.SaveChangesAsync();
                Log.DebugDatabase($"ModRole for Guild {role.Guild.Name} with ID {role.Id} resseted");
            }
            if (role.Id == gConfig.BirthdayRoleID)
            {
                gConfig.BirthdayRoleID = 0;
                await DatabaseContext.DB.SaveChangesAsync();
                Log.DebugDatabase($"BirthdayRole for Guild {role.Guild.Name} with ID {role.Id} resseted");
            }
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            DatabaseContext db = serviceProvider.GetRequiredService<DatabaseContext>();
            if (db.GuildConfigs?.FirstOrDefault(x => x.GuildID == guild.Id) == null)
            {
                db.GuildConfigs.Add(new GuildConfig(guild.Id, guild.DefaultChannel.Id));

                try
                {
                    await db.SaveChangesAsync();
                    Log.DebugInteraction("New Guild Config in DB Created. Guild " + guild.Name + " ID: " + guild.Id);
                }
                catch (Exception ex)
                {
                    Log.ErrorDatabase("Error while creating Guild Config in DB. \n" + ex.ToString()); 
                }
            }
        }

        private async Task Client_Connected()
        {
            // Remove All Slash Commands from every Guild when bool is true
            if (deleteAllGuildSlashCommands)
                await DeleteAllGuildCommands();
        }

        private async Task DeleteAllGuildCommands()
        {
            Log.DebugInteraction("Delete All Guild Slash Commands");
            foreach (var guild in client.Guilds)
            {
                try
                {
                    await client.GetGuild(guild.Id).DeleteApplicationCommandsAsync();
                    Log.DebugInteraction("All Slash Commands for Guild " + guild.Name + " with ID: " + guild.Id + " deleted!");
                }
                catch (Exception ex)
                {
                    Log.ErrorInteraction("Error Occures while deleting SlashCommands: \n" + ex);
                }
            }
        }
        Task APILog(LogMessage msg)
        {
            Log.DebugDiscord(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
