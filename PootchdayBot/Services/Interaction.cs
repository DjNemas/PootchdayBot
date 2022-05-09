using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database;
using PootchdayBot.Logging;
using System.Reflection;

namespace PootchdayBot.Services
{
    internal class Interaction
    {
        private IServiceProvider serviceProvider;
        private DiscordSocketClient client;
        private InteractionService interactionService;

        public Interaction(IServiceProvider service, DiscordSocketClient client, InteractionService interactionService)
        {
            this.serviceProvider = service;

            this.client = client;

            this.interactionService = interactionService;
        }

        public async Task InstallInteractionServiceAsync()
        {
            interactionService.Log += InteractionLog;

            interactionService.SlashCommandExecuted += InteractionService_SlashCommandExecuted;

            await interactionService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), serviceProvider);

            client.GuildAvailable += Client_GuildAvailable;

            client.InteractionCreated += Client_InteractionCreated;

            client.ChannelDestroyed += Client_ChannelDestroyed;

            client.LeftGuild += Client_LeftGuild;

        }

        private async Task Client_LeftGuild(SocketGuild guild)
        {
            // Delete Config for Guild
            DatabaseContext.DB.GuildConfigs.Remove(DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == guild.Id));

            // Delete Birthdays for Guild
            var birthdayList = DatabaseContext.DB.Birthdays.Where(x => x.GuildID == guild.Id).ToList();

            foreach (var birthday in birthdayList)
            {
                DatabaseContext.DB.Birthdays.Remove(birthday);
            }
            // Process into DB
            try
            {
                DatabaseContext.DB.SaveChanges();
                Log.DebugDatabase("Guild: " + guild.Name + " / ID: " + guild.Id + " removed from Database.");
            }
            catch (Exception ex)
            {
                Log.ErrorDatabase("A Error Occured while removing Guild " + guild.Name + " / ID: " + guild.Id + " from Database.\n" + ex.ToString());
            }
        }

        private async Task Client_ChannelDestroyed(SocketChannel channel)
        {
            var gConfig = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.AnnounceChannelID == channel.Id);
            if (gConfig != null)
            {
                SocketGuild guild = client.GetGuild(gConfig.GuildID);
                gConfig.AnnounceChannelID = guild.SystemChannel.Id;
                if (gConfig.ModRoleID != 0)
                    await guild.SystemChannel.SendMessageAsync(guild.Owner.Mention + "\n" + guild.GetRole(gConfig.ModRoleID).Mention +
                        "Der Nachrichten Channel wurde gelöscht. Ich werde die Geburtstage jetzt hier ankündigen.\n" +
                        "Bitte nutze die /einstellungen um den Channel wieder zu ändern.");
                else
                    await guild.SystemChannel.SendMessageAsync(guild.Owner.Mention + "\n" +
                        "Der Nachrichten Channel wurde gelöscht. Ich werde die Geburtstage jetzt hier ankündigen.\n" +
                        "Bitte nutze die /einstellungen um den Channel wieder zu ändern.");
            }
        }

        private async Task InteractionService_SlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if(!result.IsSuccess)
            {
                string customPrefix = result.ErrorReason.Substring(0, 3);
                if (customPrefix == "[C]")
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnknownCommand:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.ConvertFailed:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.BadArgs:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.Exception:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.Unsuccessful:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.UnmetPrecondition:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                        case InteractionCommandError.ParseFailed:
                            await context.Interaction.RespondAsync(result.ErrorReason.Substring(3));
                            break;
                    }
                }
                else
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.UnknownCommand:
                            await context.Interaction.RespondAsync("Es ist ein Fehler aufgetretten. Bitte Kontaktiere den Entwickler. Reason: UnknownCommand");
                            break;
                        case InteractionCommandError.ConvertFailed:
                            await context.Interaction.RespondAsync("Es ist ein Fehler aufgetretten. Bitte Kontaktiere den Entwickler. Reason: ConvertFail");
                            break;
                        case InteractionCommandError.BadArgs:
                            await context.Interaction.RespondAsync("Du hast falsche Argumente angegeben.");
                            break;
                        case InteractionCommandError.Exception:
                            await context.Interaction.RespondAsync("Es ist ein Fehler aufgetretten. Bitte Kontaktiere den Entwickler. Reason: Exception");
                            break;
                        case InteractionCommandError.Unsuccessful:
                            await context.Interaction.RespondAsync("Es ist ein Fehler aufgetretten. Bitte Kontaktiere den Entwickler. Reason: Unsuccessful");
                            break;
                        case InteractionCommandError.UnmetPrecondition:
                            await context.Interaction.RespondAsync("Du besitzt nicht die erforderlichen Berechtigungen.");
                            break;
                        case InteractionCommandError.ParseFailed:
                            await context.Interaction.RespondAsync("Es ist ein Fehler aufgetretten. Bitte Kontaktiere den Entwickler. Reason: ParseFailed");
                            break;
                    }
                }
            }
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            Log.DebugInteraction("Register Commands for Guild: " + guild.Name + " ID: " + guild.Id, true);

            await interactionService.RegisterCommandsToGuildAsync(guild.Id);
            
        }

        private async Task Client_InteractionCreated(SocketInteraction interaction)
        {
            InteractionContext context = new InteractionContext(client, interaction);
            await interactionService.ExecuteCommandAsync(context, serviceProvider);
        }

        private Task InteractionLog(LogMessage msg)
        {
            Log.DebugInteraction(msg.ToString(), true);
            return Task.CompletedTask;
        }
    }
}
