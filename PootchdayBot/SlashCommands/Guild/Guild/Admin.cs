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
    [DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
    public class Admin : InteractionModuleBase
    {
        [RequirePootchdayModRole()]
        [EnabledInDm(false)]
        [Group("einstellungen", "Hier kann eine Berechtigter User diverse Einstellungen vornehmen.")]
        public class EinstellungGroupModule : InteractionModuleBase
        {
            [SlashCommand("channel", "[Admin/Mod] Gib den #Channel an, an dem die Geburtstage erwähnt werden sollen.")]
            public async Task Test(SocketGuildChannel channel)
            {
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).AnnounceChannelID = channel.Id;
                await DatabaseContext.DB.SaveChangesAsync();
                await RespondAsync($"Der Channel wurde auf <#{channel.Id}> geändert.");

            }

            [Group("geburtstagsrolle", "Hier kann die @Geburtstagstolle eingestellt werden.")]
            public class Geburtstagsrolle : InteractionModuleBase
            {
                [SlashCommand("erstellen", "[Admin/Mod] Erstelle eine @Geburtstagsrolle die an Geburtstagen vergeben wird.")]
                public async Task Setze(string name)
                {
                    IRole role = Context.Guild.CreateRoleAsync(name, isHoisted: true, isMentionable: true).Result;
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).BirthdayRoleID = role.Id;
                    await RespondAsync($"Die Geburtstagsrolle wurde erstellt und auf <@&{role.Id}> gesetzt.\n" +
                        $"Du kannst in den Servereinstellungen noch die Farbe anpassen. :)");
                }

                [SlashCommand("entferne", "[Admin/Mod] Entferne die @GeburtstagsRolle aus den einstellungen.")]
                public async Task Entferne()
                {
                    ulong currentRoleID = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).BirthdayRoleID;
                    await Context.Guild.GetRole(currentRoleID).DeleteAsync();
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).BirthdayRoleID = 0;
                    await DatabaseContext.DB.SaveChangesAsync();
                    await RespondAsync($"Die Geburtstagsrolle wurde entfernt.");
                }
            }

            [Group("modrolle", "[Admin/Mod] Gib die @ModRolle an, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
            public class ModRolle : InteractionModuleBase
            {
                [SlashCommand("setze", "[Admin/Mod] Setze die @ModRolle, die Berechtigt ist die [Admin/Mod] Befehle zu nutzen.")]
                public async Task Setze(SocketRole role)
                {
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID = role.Id;
                    await DatabaseContext.DB.SaveChangesAsync();
                    await RespondAsync($"Die ModRolle wurde auf <@&{role.Id}> gesetzt.");
                }

                [SlashCommand("entferne", "[Admin/Mod] Entferne die @ModRolle aus den Einstellungen.")]
                public async Task Entferne()
                {
                    DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID = 0;
                    await DatabaseContext.DB.SaveChangesAsync();
                    await RespondAsync($"Die ModRolle wurde entfernt.");
                }
            }

            [SlashCommand("nachricht", "[Admin/Mod] Gib an wie man zum Geburtstag gegrüßt werden sollen.")]
            public async Task Nachricht(string nachricht)
            {
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).CustomMessage = nachricht;
                await DatabaseContext.DB.SaveChangesAsync();
                await RespondAsync($"Die Nachricht wurde auf `{nachricht}` gesetzt.");
            }

            [SlashCommand("ping", "[Admin/Mod] Stellt ein ob die Benutzer zum Geburtstag gepingt (@user) werden sollen oder nicht.")]
            public async Task Nachricht([Choice("An", 1), Choice("Aus", 0)] int wähle)
            {
                bool ping = Convert.ToBoolean(wähle);
                DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).Ping = ping;
                await DatabaseContext.DB.SaveChangesAsync();
                if (ping)
                    await RespondAsync($"Die Benutzer werden am Geburtstags gepingt.");
                else
                    await RespondAsync($"Die Benutzer werden am Geburtstag nicht gepingt.");
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
        [EnabledInDm(false)]
        [SlashCommand("backup", "[Admin/Mod] Erstelle ein Backup der Einstellungen und Geburtstage.")]
        public async Task Backup()
        {
            var backup = await Xml.CreateXMLBackup(Context.Guild as SocketGuild);
            await RespondWithFilesAsync(backup, text: "Ich habe dir ein Backup erstellt.\n" +
                "Bitte Speichere diese Datei/en (Download Button), falls du diese für einen Import brauchst.");
        }

        [RequirePootchdayModRole()]
        [EnabledInDm(false)]
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
                    await DatabaseContext.DB.SaveChangesAsync();
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
                    if (success)
                    {
                        await DatabaseContext.DB.SaveChangesAsync();
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

        [RequirePootchdayModRole()]
        [EnabledInDm(false)]
        [SlashCommand("postegeburtstage", "[Admin/Mod] Lass vom Bot die Geburtstage posten. Nur im Notfall nutzen!")]
        public async Task PosteGeburtstage()
        {
            GuildConfig gConfig = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id);
            List<Birthdays> birthdays = DatabaseContext.DB.Birthdays.Where(x => x.GuildID == Context.Guild.Id).ToList();

            await RemoveBirthdayRole(gConfig);

            foreach (var birthday in birthdays)
            {
                birthday.Birthday = new DateTime(DateTime.Now.Year, birthday.Birthday.Month, birthday.Birthday.Day);

                // Schaltjahr?
                if (birthday.Birthday.Month == 2 && birthday.Birthday.Day == 29)
                {
                    if (DateTime.IsLeapYear(DateTime.Today.Year))
                        birthday.Birthday = new DateTime(DateTime.Today.Year, birthday.Birthday.Month, birthday.Birthday.Day);
                    else
                        birthday.Birthday = new DateTime(DateTime.Today.Year, 3, 1);
                }
                else
                    birthday.Birthday = new DateTime(DateTime.Today.Year, birthday.Birthday.Month, birthday.Birthday.Day);

                if (birthday.Birthday == DateTime.Today)
                {
                    IGuildUser user = Context.Guild.GetUserAsync(birthday.AccountID).Result;
                    
                    await SetBirthdayRole(gConfig, birthday, user);
                    
                    string message = string.Empty;
                    if (gConfig.Ping)
                        message += user.Mention + "\n";
                    else
                        message += FormatString.HandleDiscordSpecialChar(birthday.GlobalUsername) + "\n";
                    try
                    {
                        await Context.Guild.GetTextChannelAsync(gConfig.AnnounceChannelID).Result.SendMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Log.DebugInteraction("Beim erwähnen der Geburtstagsperson ist ein Fehler Aufgetretten.\n" +
                            "DiscordServer: " + Context.Guild.Name + " ID: " + Context.Guild.Id + "\n" +
                            ex);
                    }
                }
            }
            await RespondAsync("meow");
            await DeleteOriginalResponseAsync();
        }

        private async Task RemoveBirthdayRole(GuildConfig gConfig)
        {
            var guildUsers = await Context.Guild.GetUsersAsync();
            foreach (var user in guildUsers)
            {
                foreach (var id in user.RoleIds)
                {
                    if (id == gConfig.BirthdayRoleID)
                    {
                        try
                        {
                            await user.RemoveRoleAsync(id);
                        }
                        catch (Exception ex)
                        {
                            Log.DebugInteraction("Beim entferne der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                                "DiscordServer: " + Context.Guild.Name + " ID: " + Context.Guild.Id + "\n" +
                                ex);
                            await Context.User.CreateDMChannelAsync().Result.SendMessageAsync("Beim entferne der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                                "Falls du mit dem Fehler nichts anfangen kannst, kontaktiere bitte den Entwickler oder Frage jemanden der sich mit Programmieren auskennt in deiner Community." +
                                "```" + ex + "```");
                        }
                    }
                }
            }
            Log.DebugInteraction("Birthdayrole on GuildID " + gConfig.GuildID + " resetet.");
        }

        private async Task SetBirthdayRole(GuildConfig gConfig, Birthdays birthday, IGuildUser user)
        {
            if (gConfig.BirthdayRoleID != 0)
            {
                try
                {
                    await user.AddRoleAsync(gConfig.BirthdayRoleID);
                }
                catch (Exception ex)
                {
                    Log.DebugInteraction("Beim setzen der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                        "DiscordServer: " + Context.Guild.Name + " ID: " + Context.Guild.Id + "\n" +
                        ex);
                    await Context.User.CreateDMChannelAsync().Result.SendMessageAsync("Beim setzen der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                                "Falls du mit dem Fehler nichts anfangen kannst, kontaktiere bitte den Entwickler oder Frage jemanden der sich mit Programmieren auskennt in deiner Community." +
                                "```" + ex + "```");
                }
                Log.DebugInteraction("Birthdayrole for User: " + user.Username + " on Guild " + user.Guild.Name + " setted.");
            }
        }
    }
}
