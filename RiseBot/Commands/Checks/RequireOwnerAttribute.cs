using Qmmands;
using System;
using System.Threading.Tasks;

namespace RiseBot.Commands.Checks
{
    public sealed class RequireOwnerAttribute : CheckBaseAttribute
    {
        public override Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as RiseContext;

            return Task.FromResult(context.User.Id == context.Guild.OwnerId
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("Command can only be executed by the guild owner"));
        }
    }
}
