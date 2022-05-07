using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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

            client.JoinedGuild += Client_JoinedGuild;

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

        private async Task Client_JoinedGuild(SocketGuild guild)
        {
            await guild.SystemChannel.SendMessageAsync(
                "Danke dass du mich Eingeladen hast!\n" +
                "Ab Sofort werde ich für dich die Geburstage managen. OwO\n\n" +
                "`- Bitte stelle sicher dass alle User eine Rolle haben mit der die User Slashcommands im Textchannel nutzen dürfen.\n" +
                "- Du kannst außerdem in den Servereinstellungen->Intergration->Verwalten (Bot Pootchday) einstellen in welchen Channels der Bot genutzt werden darf.`");
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
