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
                            DateTime changeToThisYear;

                            // Ist Schlatjahr?
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

                            if (changeToThisYear == DateTime.Today)
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

                            string message = string.Empty;
                            if (listBirthdays.Count > 1)
                                message = gConfig.CustomMessage == "" ? $"Heute haben gleich {listBirthdays.Count} Geburtstag! Alles Gute zum Geburtstag:\n" : gConfig.CustomMessage + "\n";
                            else
                                message = gConfig.CustomMessage == "" ? "Heute hat eine User Geburtstag! Alles Gute zum Geburtstag:\n" : gConfig.CustomMessage + "\n";

                            foreach (var birthday in birthdayGuild.Value)
                            {
                                if (gConfig.Ping)
                                    message += client.GetGuild(gConfig.GuildID).GetUser(birthday.AccountID).Mention + "\n";
                                else
                                    message += FormatString.HandleDiscordSpecialChar(birthday.GlobalUsername) + "\n";

                                await client.GetGuild(birthdayGuild.Key).GetTextChannel(gConfig.AnnounceChannelID).SendMessageAsync(message);
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
    }
}
