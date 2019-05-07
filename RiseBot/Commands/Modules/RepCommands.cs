using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands.Checks;
using RiseBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    [RequireOwner(Group = "perms")]
    [RequireRole("fwa representatives", Group = "perms")]
    public class RepCommands : RiseBase
    {
        //TODO add/remove rep role
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
                await SendMessageAsync("You are not a rep"); //TODO move into typereader
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

        [Command("outofwar")]
        public async Task RemoveWarRoleAsync()
        {
            var role = Context.Guild.GetRole(Guild.InWarRoleId);

            if(!(role is null))
            {
                await role.DeleteAsync();
            }

            var newRole = await Context.Guild.CreateRoleAsync("InWar");
            await newRole.ModifyAsync(x => x.Mentionable = true);
            Guild.InWarRoleId = newRole.Id;

            await SendMessageAsync("Role has been removed from everyone");
        }

        protected override Task AfterExecutedAsync()
        {
            Database.UpdateGuild();
            return Task.CompletedTask;
        }
    }
}
