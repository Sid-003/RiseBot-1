using Qmmands;
using System;
using System.Threading.Tasks;

namespace RiseBot.Commands.Checks
{
    public sealed class RequireOwnerAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as RiseContext;

            return context.User.Id == context.Guild.OwnerId
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command can only be executed by the guild owner");
        }
    }
}
