using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database;
using PootchdayBot.Database.Models;
using PootchdayBot.Logging;
using PootchdayBot.Services;
using PootchdayBot.SlashCommands.CustomPrecondition;
using PootchdayBot.Tools;
using System.Net;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

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

            [Group("modrolle", "[Admin/Mod] Gib die @ModRolle an, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
            public class ModRolle : InteractionModuleBase
            {
                [SlashCommand("setze", "[Admin/Mod] Setze die @ModRolle, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
                public async Task Setze(SocketRole role)
                {
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID = role.Id;
                    DatabaseContext.DB.SaveChanges();
                    await RespondAsync($"Die ModRolle wurde auf <@&{role.Id}> gesetzt.");
                }

                [SlashCommand("entferne", "[Admin/Mod] Entferne die @ModRolle, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
                public async Task Entferne()
                {
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID = 0;
                    DatabaseContext.DB.SaveChanges();
                    await RespondAsync($"Die ModRolle wurde entfernt.");
                }
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

        [RequirePootchdayModRole()]
        [SlashCommand("backup", "[Admin/Mod] Erstelle ein Backup der Einstellungen und Geburtstage.")]
        public async Task Backup()
        {
            var backup = await Xml.CreateXMLBackup(Context.Guild as SocketGuild);
            await RespondWithFilesAsync(backup, text: "Ich habe dir ein Backup erstellt.\n" +
                "Bitte Speichere diese Datei/en (Download Button), falls du diese für einen Import brauchst.");
        }

        [RequirePootchdayModRole()]
        [SlashCommand("import", "[Admin/Mod] Importiere zuvor erstellte Backups.")]
        public async Task Import(IAttachment file)
        {
            if (!(file.ContentType == "application/xml; charset=UTF-8-SIG"))
            {
                await RespondAsync($"Falscher Dateityp für Datei {file.Filename}.\n" +
                    $"Bitte nutze die Original unveränderten Backupdatei!");
                return;
            }

            Stream downloadedFile;
            using (HttpClient webClient = new HttpClient())
            {
                downloadedFile = await webClient.GetStreamAsync(new Uri(file.Url));
            }

            XmlSerializer xml = new XmlSerializer(typeof(GuildConfig));
            XmlReader reader = XmlReader.Create(downloadedFile);

            if (xml.CanDeserialize(reader))
            {
                GuildConfig? gConfigBackup = xml.Deserialize(reader) as GuildConfig;
                GuildConfig? gConfigCurrent = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == gConfigBackup.GuildID);
                try
                {
                    gConfigCurrent.AnnounceChannelID = ValidChecker.IsZero(gConfigBackup.AnnounceChannelID);
                    gConfigCurrent.CustomMessage = gConfigBackup.CustomMessage;
                    gConfigCurrent.ModRoleID = gConfigBackup.ModRoleID;
                    gConfigCurrent.Ping = gConfigBackup.Ping;
                    DatabaseContext.DB.SaveChanges();
                    await RespondAsync("Alle Einstellungen erfolgreich geladen! c:");
                    return;
                }
                catch
                {
                    Log.DebugDatabase("GuildConfigBackup could not be deserializied.");
                }
            }
            else
                Log.DebugDatabase("GuildConfigBackup could not be deserializied.");

            xml = new XmlSerializer(typeof(List<Birthdays>));
            if (xml.CanDeserialize(reader))
            {
                List<Birthdays>? birthdayBackup = xml.Deserialize(reader) as List<Birthdays>;
                if (birthdayBackup.Count > 0)
                {
                    List<Birthdays> oldBirthdayList = DatabaseContext.DB.Birthdays.Where(x => x.GuildID == birthdayBackup[0].GuildID).ToList();

                    foreach (var birthday in oldBirthdayList)
                    {
                        DatabaseContext.DB.Birthdays.Remove(birthday);
                    }

                    bool success = true;
                    foreach (var birthdayB in birthdayBackup)
                    {
                        try
                        {
                            Birthdays birthday = new Birthdays(birthdayB.AccountID, birthdayB.GuildID, birthdayB.GlobalUsername, birthdayB.Birthday);
                            DatabaseContext.DB.Birthdays.Add(birthday);
                        }
                        catch
                        {
                            Log.DebugDatabase("BirthdayBackup could not be deserializied.");
                            success = false;
                            break;
                        }
                    }
                    if(success)
                    {
                        DatabaseContext.DB.SaveChanges();
                        await RespondAsync("Alle Geburtstage erfolgreich geladen! c:");
                    }
                    else
                    {
                        await RespondAsync("Es wurde nicht die richtige Backupdatei mitgeschickt.\n" +
                            $"Bitte nutze die Original unveränderten Backupdatei!");
                    }
                    return;
                }
            }
            else
                Log.DebugDatabase("BirthdayBackup could not be deserializied.");

            await RespondAsync("Es wurde nicht die richtige Backupdatei mitgeschickt.\n" +
                $"Bitte nutze die Original unveränderten Backupdatei!");
        }
    }
}
