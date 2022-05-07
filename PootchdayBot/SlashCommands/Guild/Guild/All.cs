﻿using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database.Models;
using PootchdayBot.Database;

namespace PootchdayBot.SlashCommands
{
    public class All : InteractionModuleBase
    {
        [SlashCommand("info", "[Jeder] Info über diesen Bot")]
        public async Task Info()
        {
            SocketGuildUser nemas = Context.Guild.GetUsersAsync().Result.FirstOrDefault(x => x.Id == 123613862237831168) as SocketGuildUser;

            string stringNemas = nemas == null ? "Nemas#0185" : nemas.Mention;

            await RespondAsync("Ich speichere, wenn Ihr möchtet, eure Geburtstage und erwähne euch an eurem Geburtstag. Somit bekommt jeder mit wann Ihr Geburtstag habt. :D\n\n" +
                $"Entwickelt wurde ich von {stringNemas}.\n" +
                "Falls Ihr irgendwelche Probleme haben solltet meldet euch doch gerne bei Ihm. :)\n" +
                "Ihr mögt mich? Dann lasst doch gerne eine kleine Spende da und supportet mein Schöpfer bei weiteren Projekten. <3 https://paypal.me/djnemas");
        }

        [SlashCommand("setze", "[Jeder] Trage dein Geburstag ein. (Das Jahr wird nur zur Ermittlung eines Schaltjahres genutzt!)")]
        public async Task Setze(int tag, Monat monat, int jahr)
        {
            if(DatabaseContext.DB.Birthdays.FirstOrDefault(x => x.AccountID == Context.User.Id && x.GuildID == Context.Guild.Id) != null)
            {
                await RespondAsync("Du bist bereits eingetragen.");
                return;
            }

            DateTime dt;
            try
            {
                dt = new DateTime(jahr, (int)monat, tag);
            }
            catch (Exception)
            {
                await RespondAsync("Du hast ein Ungültiges Datum angegeben. Bitte versuche es erneut.");
                return;
            }

            DatabaseContext.DB.Birthdays.Add(new Birthdays(Context.User.Id, Context.Guild.Id, Context.User.Username, dt));
            DatabaseContext.DB.SaveChanges();

            await RespondAsync("Du wurdest eingetragen.");
        }

        [SlashCommand("entferne", "[Jeder] Entferne dein B-Day eintrag von dem Server.")]
        public async Task Entferne()
        {
            var user = DatabaseContext.DB.Birthdays.FirstOrDefault(x => x.AccountID == Context.User.Id && x.GuildID == Context.Guild.Id);

            if (user == null)
            {
                await RespondAsync("Du hast auf dem Server keinen Eintrag.");
                return;
            }

            DatabaseContext.DB.Birthdays.Remove(user);
            DatabaseContext.DB.SaveChanges();

            await RespondAsync("Du wurdest auser der Datenbank entfernt.");
        }

        [SlashCommand("letzte", "[Jeder] Zeigt Geburtstage der letzten 30 Tage an.")]
        public async Task Letze()
        {
            await ErmittleLetzteKommendeGeburtstage(true);
        }

        [SlashCommand("kommende", "[Jeder] Zeigt Geburtstage der nächsten 30 Tage an.")]
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
                        listBirthdays.Add(birthdays);
                }
                else
                {
                    if (userBirthday <= DateTime.Now.AddDays(30) && userBirthday >= DateTime.Now)
                        listBirthdays.Add(birthdays);
                }
            }
            if (listBirthdays.Count > 0)
            {
                string message = string.Empty;

                if (letzte)
                    message = "In den letzten 30 Tagen hatten folgende User Geburtstag:\n";
                else
                    message = "In den kommenden 30 Tagen haben folgende User Geburtstag:\n";

                foreach (var birthdays in listBirthdays)
                {
                    message += $"{birthdays.GlobalUsername}";
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

        [SlashCommand("help", "[Jeder] Ich schicke dir eine Nachricht die dir helfen sollte. :P")]
        public async Task Help()
        {
            var dmChannel = Context.User.CreateDMChannelAsync().Result;
            await dmChannel.SendMessageAsync("Du findest alle Befehle des Bots im für den Bot aktivierten Channel auf einem Server.\n\n" +
                "Für die Admins/Mods:\n" +
                "Mit den /einstellungs Slashbefehlen könnt Ihr den Bot konfigurieren.");
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
        Dezember = 12
    }
}
