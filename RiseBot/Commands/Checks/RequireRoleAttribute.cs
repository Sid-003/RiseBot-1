using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Checks
{
    public sealed class RequireRoleAttribute : CheckBaseAttribute
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override Task<CheckResult> CheckAsync(ICommandContext originalContext, IServiceProvider provider)
        {
            var context = originalContext as RiseContext;

            var user = context.User;

            return Task.FromResult(
                (from role in user.Roles
                    from _role in _roles
                    where string.Equals(role.Name, _role, StringComparison.InvariantCultureIgnoreCase)
                    select role).Any()
                    ? CheckResult.Successful
                    : CheckResult.Unsuccessful("You don't have the necessary role to execute this command"));
        }
    }
}
