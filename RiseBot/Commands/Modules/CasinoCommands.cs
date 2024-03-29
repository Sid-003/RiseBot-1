﻿using BandWrapper;
using Qmmands;
using RiseBot.Commands.Checks;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    [RequireOwner]
    public class CasinoCommands : RiseBase
    {
        public BandClient Band { get; set; }

        //TODO eval

        //TODO this is shit
        [Command("dbcleanse")]
        public Task CleanseDbAsync()
        {
            var guildMembers = Guild.GuildMembers;

            var removed = 0;
            for (var i = 0; i < guildMembers.Count; i++)
            {
                var foundMember = Context.Guild.Users.Any(x => x.Id == guildMembers[i].Id);

                if (foundMember)
                    continue;

                guildMembers.RemoveAt(i);
                removed++;
            }

            return SendMessageAsync($"Successfully cleansed {removed} entries");
        }

        [Command("getquota")]
        public Task GetQuotaAsync()
        {
            return SendMessageAsync($"{Band.GetQuota()}");
        }
    }
}
