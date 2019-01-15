using ClashWrapper;
using ClashWrapper.Entities.War;
using Qmmands;
using RiseBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    public class MiscCommands : RiseBase
    {
        public ClashClient Clash { get; set; }
        public StartTimeService Start { get; set; }
        public CommandService CommandService { get; set; }
        public IServiceProvider Services { get; set; }
        
        [Command("ping")]
        public Task PingAsync()
        {
            return SendMessageAsync("pong");
        }

        [Command("setname")]
        [RunMode(RunMode.Parallel)]
        public async Task SetNameAsync(string userTag)
        {
            var guildMember = Guild.GuildMembers.FirstOrDefault(x => x.Id == Context.User.Id); //TODO typereader

            if (guildMember is null)
            {
                await SendMessageAsync("You are not verified");
                return;
            }

            if (!guildMember.Tags.Any(x => string.Equals(x, userTag, StringComparison.InvariantCultureIgnoreCase)))
            {
                await SendMessageAsync("This tag is not registered to your account");
                return;
            }

            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);

            var foundMember = clanMembers.FirstOrDefault(x =>
                string.Equals(x.Tag, userTag, StringComparison.InvariantCultureIgnoreCase));

            if (foundMember is null)
            {
                await SendMessageAsync("This tag does not belong to the clan");
                return;
            }

            guildMember.MainTag = foundMember.Tag;

            await Database.WriteEntityAsync(Guild);
            await Context.User.ModifyAsync(x => x.Nickname = foundMember.Name);
            await SendMessageAsync("Name has been set");
        }

        [Command("clear", "c")]
        public Task ClearAsync(int count = 5)
            => Message.DeleteMessagesAsync(Context, count + 1);

        [Command("discordcheck")]
        [RunMode(RunMode.Parallel)]
        public async Task DiscordCheck()
        {
            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);
            var discordMembers = Guild.GuildMembers;

            var missingMembers = clanMembers.Where(clanMember => !discordMembers.Any(x =>
                    x.Tags.Any(y => string.Equals(y, clanMember.Tag, StringComparison.InvariantCultureIgnoreCase))))
                .ToArray();

            var missingList = string.Join('\n', missingMembers.Select(x => $"{x.Name}{x.Tag}"));

            await SendMessageAsync(missingList);
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var modules = CommandService.GetAllModules();
            var commandMap = new Dictionary<Module, IEnumerable<Command>>();

            foreach (var module in modules)
            {
                var result = await module.RunChecksAsync(Context, Services);

                if (!result.IsSuccessful) continue;

                var filtered = new List<Command>();

                foreach (var command in module.Commands)
                {
                    result = await command.RunChecksAsync(Context, Services);

                    if(result.IsSuccessful)
                        filtered.Add(command);
                }

                commandMap[module] = filtered;
            }

            var sb = new StringBuilder();

            foreach (var module in commandMap.Keys)
            {
                sb.AppendLine($"#{module.Name}");

                foreach (var command in commandMap[module])
                    sb.AppendLine($"\t-{command.FullAliases.First()} {string.Join(' ', command.Parameters.Select(x => $"[{x.Name}]"))}");
            }

            await SendMessageAsync($"```css\n{sb}```");
        }

        [Command("mytags")]
        [RunMode(RunMode.Parallel)]
        public async Task GetTagsAsync()
        {
            var guildMember = Guild.GuildMembers.FirstOrDefault(x => x.Id == Context.User.Id);

            if (guildMember is null)
            {
                await SendMessageAsync("You aren't verified");
                return;
            }

            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);

            var matchingTags = (from tag in guildMember.Tags
                let foundMember = clanMembers.FirstOrDefault(x => string.Equals(x.Tag, tag, StringComparison.InvariantCultureIgnoreCase))
                select foundMember is null
                    ? $"{tag} - Not in clan"
                    : $"{foundMember.Tag} - {foundMember.Name}").ToList();

            await SendMessageAsync(string.Join('\n', matchingTags));
        }

        [Command("band")]
        [RunMode(RunMode.Parallel)]
        public async Task GetBandMatchAsync()
        {
            var currentWar = await Clash.GetCurrentWarAsync(Guild.ClanTag);

            if (currentWar is null || currentWar.State != WarState.Preparation)
            {
                await SendMessageAsync("Either not in war or the API hasn't updated yet");
                return;
            }

            await SendMessageAsync($"!match {currentWar.Opponent.Tag}");
        }
    }
}
