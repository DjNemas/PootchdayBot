using Discord.WebSocket;
using PootchdayBot.Database;
using PootchdayBot.Database.Models;
using PootchdayBot.Logging;
using PootchdayBot.Tools;

namespace PootchdayBot.BirthdayFunctions
{
    public class BirthdayReminder
    {
        private readonly DiscordSocketClient client;

        public BirthdayReminder(DiscordSocketClient client)
        {
            this.client = client;
        }

        public void StartReminder()
        {
            Task reminder = Task.Run(async () =>
            {
                while (true)
                {
                    DateTime nextDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1).ToUniversalTime();
                    TimeSpan span = nextDay.Subtract(DateTime.UtcNow);

                    Log.DebugInteraction("Wait Reminder");
                    await Task.Delay(span);

                    Log.DebugInteraction("Start Reminder");
                    Dictionary<ulong, List<Birthdays>> listBirthdays = new Dictionary<ulong, List<Birthdays>>();
                    try
                    {
                        foreach (var birthday in DatabaseContext.DB.Birthdays)
                        {
                            DateTime birthdayDate = ChangeToThisYear(birthday);

                            // If Date is Today Fill Dictonary. Key = Guild ID
                            if (birthdayDate == DateTime.Today)
                            {
                                if (!listBirthdays.ContainsKey(birthday.GuildID))
                                {
                                    listBirthdays.Add(birthday.GuildID, new List<Birthdays>());
                                    listBirthdays[birthday.GuildID].Add(birthday);
                                }
                                else
                                {
                                    listBirthdays[birthday.GuildID].Add(birthday);
                                }
                            }
                        }

                        foreach (var birthdayGuild in listBirthdays)
                        {
                            GuildConfig gConfig = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == birthdayGuild.Key);
                            // Remove Birthdayrole from last Birthdays

                            await RemoveBirthdayRole(gConfig);

                            // Create Message Header based on how much User have Birthday on current day.
                            string message = string.Empty;
                            if (listBirthdays.Count > 1)
                                message = gConfig.CustomMessage == "" ? $"Heute haben gleich {listBirthdays.Count} Geburtstag! Alles Gute zum Geburtstag:\n" : gConfig.CustomMessage + "\n";
                            else
                                message = gConfig.CustomMessage == "" ? "Heute hat eine User Geburtstag! Alles Gute zum Geburtstag:\n" : gConfig.CustomMessage + "\n";

                            // Expand message with users birthday
                            foreach (var birthday in birthdayGuild.Value)
                            {
                                await SetBirthdayRole(gConfig, birthday);

                                if (gConfig.Ping)
                                    message += client.GetGuild(gConfig.GuildID).GetUser(birthday.AccountID).Mention + "\n";
                                else
                                    message += FormatString.HandleDiscordSpecialChar(birthday.GlobalUsername) + "\n";
                                try
                                {
                                    await client.GetGuild(birthdayGuild.Key).GetTextChannel(gConfig.AnnounceChannelID).SendMessageAsync(message);
                                }
                                catch (Exception ex)
                                {
                                    Log.DebugInteraction("Beim erwähnen der Geburtstagsperson ist ein Fehler Aufgetretten.\n" +
                                        "DiscordServer: " + client.GetGuild(birthdayGuild.Key).Name + " ID: " + birthdayGuild.Key + "\n" +
                                        ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorInteraction("Error beim Reminder Task:\n" + ex.ToString());
                    }
                }
            });
        }

        private async Task RemoveBirthdayRole(GuildConfig gConfig)
        {
            var list = client.GetGuild(gConfig.GuildID).Users;
            foreach (var user in list)
            {
                foreach (var role in user.Roles)
                {
                    if(role.Id == gConfig.BirthdayRoleID)
                    {
                        try
                        {
                            await user.RemoveRoleAsync(role);
                        }
                        catch (Exception ex)
                        {
                            Log.DebugInteraction("Beim entferne der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                                "DiscordServer: " + client.GetGuild(gConfig.GuildID).Name + " ID: " + gConfig.GuildID + "\n" +
                                ex);
                        }
                    }
                        
                }
            }
            Log.DebugInteraction("Birthdayrole on GuildID " + gConfig.GuildID + " resetet.");
        }

        private async Task SetBirthdayRole(GuildConfig gConfig, Birthdays birthday)
        {
            if (gConfig.BirthdayRoleID != 0)
            {
                SocketGuildUser user = client.GetGuild(birthday.GuildID).GetUser(birthday.AccountID);
                try
                {
                    await user.AddRoleAsync(gConfig.BirthdayRoleID);
                }
                catch (Exception ex)
                {
                    Log.DebugInteraction("Beim setzen der Birthdayrolle ist ein Fehler Aufgetretten.\n" +
                        "DiscordServer: " + client.GetGuild(gConfig.GuildID).Name + " ID: " + gConfig.GuildID + "\n" +
                        ex);
                }
                Log.DebugInteraction("Birthdayrole for User: " + user.Username + " on Guild " + user.Guild.Name + " setted.");
            }
                
        }
        private DateTime ChangeToThisYear(Birthdays birthday)
        {
            DateTime changeToThisYear;
            // Ist Schaltjahr?
            if (birthday.Birthday.Month == 2 && birthday.Birthday.Day == 29)
            {
                if (DateTime.IsLeapYear(DateTime.Today.Year))
                    changeToThisYear = new DateTime(DateTime.Today.Year, birthday.Birthday.Month, birthday.Birthday.Day);
                else
                    changeToThisYear = new DateTime(DateTime.Today.Year, 3, 1);
            }
            else
            {
                changeToThisYear = new DateTime(DateTime.Today.Year, birthday.Birthday.Month, birthday.Birthday.Day);
            }
            return changeToThisYear;
        }
    }
}
