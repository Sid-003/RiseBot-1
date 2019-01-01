using System;
using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands.Checks;
using System.Linq;
using System.Threading.Tasks;
using RiseBot.Services;

namespace RiseBot.Commands.Modules
{
    [RequireOwner(Group = "perms")]
    [RequireRole("fwa representatives", Group = "perms")]
    public class RepCommands : RiseBase
    {
        public StartTimeService Start { get; set; }

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
        public async Task SetTimezoneAsync(double timezone)
        {
            var rep = Guild.FWAReps.FirstOrDefault(x => x.Id == Context.User.Id);

            if (rep is null)
            {
                await SendMessageAsync("You are not a rep");
                return;
            }

            rep.TimeZone = timezone;

            var (start, end) = Start.GetStartEndTimes();

            var times = Guild.FWAReps.ToDictionary(x => x.Id,
                x => (start.Add(TimeSpan.FromHours(x.TimeZone)),
                    end.Add(TimeSpan.FromHours(x.TimeZone))));

            await Start.UpdateLastMessageAsync(times);
            await SendMessageAsync("Timezone has been set");
        }

        [Command("timezone")]
        public Task ViewTimeZoneAsync()
        {
            var rep = Guild.FWAReps.FirstOrDefault(x => x.Id == Context.User.Id);

            return SendMessageAsync(rep is null ? "You are not a rep" : $"Your timezone is {rep.TimeZone}");
        }

        protected override Task AfterExecutedAsync(Command command)
        {
            return Database.WriteEntityAsync(Guild);
        }
    }
}
