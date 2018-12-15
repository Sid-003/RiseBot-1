using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands.Checks;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    [RequireOwner(Group = "perms")]
    [RequireRole("fwa representative", Group = "perms")]
    public class RepCommands : RiseBase
    {
        [Command("addrep")]
        public Task AddRepAsync(SocketGuildUser user, double timezone)
        {
            Guild.FWAReps.Add(new FWARep
            {
                Id = user.Id,
                TimeZone = timezone
            });

            return SendMessageAsync("Rep has been added");
        }

        [Command("removerep")]
        public Task RemoveRepAsync(SocketGuildUser user)
        {
            var rep = Guild.FWAReps.FirstOrDefault(x => x.Id == user.Id);
            Guild.FWAReps.Remove(rep);

            return SendMessageAsync("Rep has been removed");
        }

        [Command("settimezone")]
        public Task SetTimezoneAsync(double timezone)
        {
            var rep = Guild.FWAReps.FirstOrDefault(x => x.Id == Context.User.Id);
            rep.TimeZone = timezone;

            return SendMessageAsync("Timezone has been set");
        }

        protected override Task AfterExecutedAsync(Command command)
        {
            return Database.WriteEntityAsync(Guild);
        }
    }
}
