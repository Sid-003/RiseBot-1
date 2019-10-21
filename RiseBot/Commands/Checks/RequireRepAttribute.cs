using Qmmands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RiseBot.Services;
using System.Linq;

namespace RiseBot.Commands.Checks
{
    public class RequireRepAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext ctx)
        {
            var context = (RiseContext)ctx;
            var db = ctx.ServiceProvider.GetService<DatabaseService>();

            var reps = db.Guild.FWAReps;

            return reps.Any(x => x.Id == context.User.Id) ? CheckResult.Successful : CheckResult.Unsuccessful("You don't have access to these commands");
        }
    }
}
