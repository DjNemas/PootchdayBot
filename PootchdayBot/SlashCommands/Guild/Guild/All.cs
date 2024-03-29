﻿using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database.Models;
using PootchdayBot.Database;
using System.Globalization;
using Discord;
using PootchdayBot.Tools;
using System.Text;

namespace PootchdayBot.SlashCommands
{
    [DefaultMemberPermissions(GuildPermission.UseApplicationCommands)]
    public class All : InteractionModuleBase
    {
        [SlashCommand("info", "[Jeder] Info über diesen Bot")]
        [EnabledInDm(true)]
        public async Task Info()
        {
            SocketGuildUser nemas = Context.Guild.GetUsersAsync().Result.FirstOrDefault(x => x.Id == 123613862237831168) as SocketGuildUser;

            string stringNemas = nemas == null ? "Nemas#0185" : nemas.Mention;

            await RespondAsync("Ich speichere, wenn Ihr möchtet, eure Geburtstage und erwähne euch an eurem Geburtstag. Somit bekommt jeder mit wann Ihr Geburtstag habt. :D\n\n" +
                $"Entwickelt wurde ich von {stringNemas}.\n" +
                "Falls Ihr irgendwelche Probleme haben solltet meldet euch doch gerne bei Ihm. :)\n" +
                "Ihr mögt mich? Dann lasst doch gerne eine kleine Spende da und supportet mein Schöpfer bei weiteren Projekten. <3 https://paypal.me/djnemas");
        }

        [SlashCommand("eintragen", "[Jeder] Trage dein Geburstag ein. (Jahr is Optional)")]
        [EnabledInDm(false)]
        public async Task Eintragen(int tag, Monat monat, int? jahr = null)
        {
            if (DatabaseContext.DB.Birthdays.FirstOrDefault(x => x.AccountID == Context.User.Id && x.GuildID == Context.Guild.Id) != null)
            {
                await RespondAsync("Du bist bereits eingetragen.");
                return;
            }

            DateTime dt;
            try
            {
                if (jahr == null)
                {
                    dt = new DateTime(2020, (int)monat, tag);
                }
                else
                {
                    dt = new DateTime(Convert.ToInt32(jahr), (int)monat, tag);
                }
            }
            catch (Exception)
            {
                await RespondAsync("Du hast ein Ungültiges Datum angegeben. Bitte versuche es erneut.");
                return;
            }

            DatabaseContext.DB.Birthdays.Add(new Birthdays(Context.User.Id, Context.Guild.Id, Context.User.Username, dt));
            await DatabaseContext.DB.SaveChangesAsync();

            await RespondAsync("Danke, dass du dich eingetragen hast!");
        }

        [SlashCommand("entferne", "[Jeder] Entferne dein B-Day Eintrag von dem Server.")]
        [EnabledInDm(false)]
        public async Task Entferne()
        {
            var user = DatabaseContext.DB.Birthdays.FirstOrDefault(x => x.AccountID == Context.User.Id && x.GuildID == Context.Guild.Id);

            if (user == null)
            {
                await RespondAsync("Du hast auf dem Server keinen Eintrag.");
                return;
            }

            DatabaseContext.DB.Birthdays.Remove(user);
            await DatabaseContext.DB.SaveChangesAsync();

            await RespondAsync("Du wurdest aus der Datenbank entfernt.");
        }

        [SlashCommand("alle", "[Jeder] Zeigt alle eingetragenen Geburtstage an.")]
        [EnabledInDm(false)]
        public async Task Alle()
        {
            List<Birthdays> listBirthdays = DatabaseContext.DB.Birthdays.Where(x => x.GuildID == Context.Guild.Id).ToList();
            // Set Every Yeah to 10 for Oder List
            foreach (var birthday in listBirthdays)
            {
                birthday.Birthday = new DateTime(10, birthday.Birthday.Month, birthday.Birthday.Day);
            }
            listBirthdays.Sort((x, y) =>  x.Birthday.CompareTo(y.Birthday));

            string message = "";
            List<string> listMessages = new List<string>();
            CultureInfo ci = new CultureInfo("de-DE");
            foreach (var birthday in listBirthdays)
            {
                string temp = $"{FormatString.HandleDiscordSpecialChar(birthday.GlobalUsername)} {birthday.Birthday.ToString("dd. MMMM", ci)}\n";
                if ((message.Length + temp.Length) <= 2000)
                    message += temp;
                else
                {
                    listMessages.Add(message);
                    message = temp;
                }
            }
            listMessages.Add(message);
            await RespondAsync(listMessages[0]);
            for (int i = 1; i < listMessages.Count; i++)
            {
                await FollowupAsync(listMessages[i]);
            }
        }

        [SlashCommand("letzte", "[Jeder] Zeigt Geburtstage der letzten 30 Tage an.")]
        [EnabledInDm(false)]
        public async Task Letze()
        {
            await ErmittleLetzteKommendeGeburtstage(true);
        }

        [SlashCommand("kommende", "[Jeder] Zeigt Geburtstage der nächsten 30 Tage an.")]
        [EnabledInDm(false)]
        public async Task Kommende()
        {
            await ErmittleLetzteKommendeGeburtstage(false);
        }

        private async Task ErmittleLetzteKommendeGeburtstage(bool letzte)
        {
            var birthdaysDB = DatabaseContext.DB.Birthdays;

            List<Birthdays> listBirthdays = new List<Birthdays>();

            foreach (var birthdays in birthdaysDB)
            {
                DateTime userBirthday = new DateTime(DateTime.Now.Year, birthdays.Birthday.Month, birthdays.Birthday.Day);
                if (letzte)
                {
                    if (userBirthday >= DateTime.Now.AddDays(-31) && userBirthday <= DateTime.Now)
                    {
                        birthdays.Birthday = new DateTime(10, birthdays.Birthday.Month, birthdays.Birthday.Day);
                        listBirthdays.Add(birthdays);
                    }
                }
                else
                {
                    if (userBirthday <= DateTime.Now.AddDays(30) && userBirthday >= DateTime.Now)
                    {
                        birthdays.Birthday = new DateTime(10, birthdays.Birthday.Month, birthdays.Birthday.Day);
                        listBirthdays.Add(birthdays);
                    }
                }
            }
            if (listBirthdays.Count > 0)
            {
                listBirthdays.Sort((x, y) => x.Birthday.CompareTo(y.Birthday));
                string message = string.Empty;

                if (letzte)
                    message = "In den letzten 30 Tagen hatten folgende User Geburtstag:\n";
                else
                    message = "In den kommenden 30 Tagen haben folgende User Geburtstag:\n";

                if(letzte)
                    foreach (var birthdays in listBirthdays)
                    {
                        message += $"{FormatString.HandleDiscordSpecialChar(birthdays.GlobalUsername)} hatte am {birthdays.Birthday.ToString("dd. MMMM", new CultureInfo("de-DE"))} Geburtstag. <:kagoparty:859338304171016202> \n";
                    }
                else
                    foreach (var birthdays in listBirthdays)
                    {
                        message += $"{FormatString.HandleDiscordSpecialChar(birthdays.GlobalUsername)} hat am {birthdays.Birthday.ToString("dd. MMMM", new CultureInfo("de-DE"))} Geburtstag. <:kagoparty:859338304171016202> \n";
                    }
                await RespondAsync(message);
            }
            else
            {
                if (letzte)
                    await RespondAsync("Es gab keine Geburtstage in den letzten 30 Tagen.");
                else
                    await RespondAsync("Es gibt keine Geburtstage in den nächsten 30 Tagen.");
            }
        }
        
        [SlashCommand("hilfe", "[Jeder] Ich schicke dir eine Nachricht die dir helfen sollte. :P")]
        [EnabledInDm(true)]
        public async Task Hilfe()
        {
            var dmChannel = Context.User.CreateDMChannelAsync().Result;

            ulong modID = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == Context.Guild.Id).ModRoleID;
            if (Context.User.Id == Context.Guild.OwnerId || Context.Guild.GetUserAsync(Context.User.Id).Result.RoleIds.Contains(modID))
            {
                List<FileAttachment> files = new List<FileAttachment>();
                files.Add(new FileAttachment(new FileStream("./HelpImages/helpimage1.png", FileMode.Open), "helpimage1.png", "Bild1"));
                files.Add(new FileAttachment(new FileStream("./HelpImages/helpimage2.png", FileMode.Open), "helpimage2.png", "Bild2"));
                files.Add(new FileAttachment(new FileStream("./HelpImages/helpimage3.png", FileMode.Open), "helpimage3.png", "Bild3"));
                files.Add(new FileAttachment(new FileStream("./HelpImages/helpimage4.png", FileMode.Open), "helpimage4.png", "Bild4"));

                await dmChannel.SendFilesAsync(files, "Du findest alle Befehle des Bots, im für den Bot aktivierten Kanal, auf einem Server mit Beschreibung.\n\n" +
                "Du kannst mit den /einstellung Slashbefehlen den Bot nach belieben Konfigurieren. (siehe erstes Bild)\n" +
                "Wenn du einstellen möchtest in welchem Channel und mit welchen Rollen der Bot genutzt werden darf, kannst du dies in den Servereinstellungen->Intergration einstellen. (siehe zweites Bild)\n" +
                "Du kannst natürlich auch ganz übertreiben und jeglichen Slashbefehl individuell einstellen. x) Dies geht ebenfalls über die Servereinstellungen->Intergration. (siehe drittes/viertes Bild)");
            }
            else
            {
                FileAttachment file = new FileAttachment(new FileStream("./HelpImages/helpimageeveryone1.png", FileMode.Open), "helpimageeveryone1.png", "Bild1");
                await dmChannel.SendFileAsync(file, "Du findest alle Befehle des Bots, im für den Bot aktivierten Kanal, auf einem Server mit Beschreibung. (siehe Bild)");
            }
            await RespondAsync("meow");
            await DeleteOriginalResponseAsync();
        
        }
    }

    public enum Monat
    {
        Januar = 1,
        Februar = 2,
        März = 3,
        April = 4,
        Mai = 5,
        Juni = 6,
        Juli = 7,
        August = 8,
        September = 9,
        Oktober = 10,
        Novermber = 11,
        Dezember = 12,
        [ChoiceDisplay("1")]
        One = 1,
        [ChoiceDisplay("2")]
        Two = 2,
        [ChoiceDisplay("3")]
        Three = 3,
        [ChoiceDisplay("4")]
        Four = 4,
        [ChoiceDisplay("5")]
        Five = 5,
        [ChoiceDisplay("6")]
        Six = 6,
        [ChoiceDisplay("7")]
        Seven = 7,
        [ChoiceDisplay("8")]
        Eight = 8,
        [ChoiceDisplay("9")]
        Nine = 9,
        [ChoiceDisplay("10")]
        Ten = 10,
        [ChoiceDisplay("11")]
        Eleven = 11,
        [ChoiceDisplay("12")]
        Twelve = 12,
    }
}
