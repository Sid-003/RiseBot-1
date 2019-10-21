using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Checks
{
    public sealed class RequireRoleAttribute : CheckAttribute
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext originalContext)
        {
            var context = originalContext as RiseContext;

            var user = context.User;

            return user.Roles.Select(x => x.Name).Intersect(_roles, StringComparer.InvariantCultureIgnoreCase).Any()
                 ? CheckResult.Successful
                 : CheckResult.Unsuccessful("You don't have the necessary role to execute this command?");
        }
    }
}
