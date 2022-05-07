using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PootchdayBot.Database;

namespace PootchdayBot.SlashCommands.CustomPrecondition
{
    public class RequirePootchdayModRoleAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (context.User is SocketGuildUser gUser)
            {
                ulong modRollID = DatabaseContext.DB.GuildConfigs.FirstOrDefault(x => x.GuildID == context.Guild.Id).ModRoleID;

                if (modRollID != null && gUser.Roles.Any(r => r.Id == modRollID) || gUser.Id == gUser.Guild.Owner.Id)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                {
                    if (modRollID != 0)
                        return Task.FromResult(PreconditionResult.FromError($"[C]Du musst die Rolle <@&{context.Guild.GetRole(modRollID).Id}> besitzen oder Owner sein um den Befehl nutzen zu dürfen."));
                    else
                        return Task.FromResult(PreconditionResult.FromError($"[C]Nur der Owner darf diesen Befehl nutzen."));
                }
                    
            }
            return Task.FromResult(PreconditionResult.FromError("[C]Du musst dich für den Befehl auf einem Server befinden."));
        }
    }
}
