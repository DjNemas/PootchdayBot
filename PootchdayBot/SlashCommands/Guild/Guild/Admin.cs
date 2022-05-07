using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database;
using PootchdayBot.SlashCommands.CustomPrecondition;

namespace PootchdayBot.SlashCommands
{
    public class Admin : InteractionModuleBase
    {
        [RequirePootchdayModRole()]
        [Group("einstellungen", "Hier kann eine Berechtigter User diverse einstellungen vornehmen.")]
        public class EinstellungGroupModule : InteractionModuleBase
        {
            [SlashCommand("channel", "[Admin/Mod] Gib den #Channel an, an dem die Geburtstagskinder erwähnt werden sollen.")]
            public async Task Test(SocketGuildChannel channel)
            {
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).AnnounceChannelID = channel.Id;
                DatabaseContext.DB.SaveChanges();
                await RespondAsync($"Der Channel wurde auf <#{channel.Id}> geändert.");
                
            }

            [SlashCommand("modrolle", "[Admin/Mod] Gib die @ModRolle an, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
            public async Task ModRolle(SocketRole role)
            {
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID = role.Id;
                DatabaseContext.DB.SaveChanges();
                await RespondAsync($"Die ModRolle wurde auf <@&{role.Id}> gesetzt.");
            }

            [SlashCommand("nachricht", "[Admin/Mod] Gib an wie die Geburtstagskinder gegrüßt werden sollen.")]
            public async Task Nachricht(string nachricht)
            {
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).CustomMessage = nachricht;
                DatabaseContext.DB.SaveChanges();
                await RespondAsync($"Die Nachricht wurde auf `{nachricht}` gesetzt.");
            }

            [SlashCommand("ping", "[Admin/Mod] Stellt ein ob die Geburtstagskinder gepingt (@user) werden sollen oder nicht.")]
            public async Task Nachricht([Choice("An", 1), Choice("Aus", 0)] int wähle)
            {
                bool ping = Convert.ToBoolean(wähle);
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).Ping = ping;
                DatabaseContext.DB.SaveChanges();
                if (ping)
                    await RespondAsync($"Die Geburtstagskinder werden nun gepingt.");
                else
                    await RespondAsync($"Die Geburtstagskinder werden nicht gepingt.");
            }

            [SlashCommand("list", "[Admin/Mod] Zeigt die aktuellen Einstellungen vom Pootchday an.")]
            public async Task List()
            {
                var gConfig = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id);

                string modRolle = gConfig.ModRoleID == 0 ? "Nicht gesetzt" : "<@&" + gConfig.ModRoleID + ">";
                string nachrichtGesetzt = gConfig.CustomMessage == "" ? "Nicht gesetzt" : gConfig.CustomMessage;
                string pingAnAus = gConfig.Ping == true ? "An" : "Aus";

                await RespondAsync("Benachrichtigungschannel: <#" + gConfig.AnnounceChannelID + ">\n" +
                    "ModRolle: " + modRolle + "\n" +
                    "Nachricht: " + nachrichtGesetzt + "\n" +
                    "Ping: " + pingAnAus + "\n");
            }
        }

    }
}
