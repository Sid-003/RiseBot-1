using ClashWrapper;
using Qmmands;
using RiseBot.Results;
using RiseBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashWrapper.Entities.ClanMembers;

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
        public async Task SetNameAsync(string userTag)
        {
            var guildMember = Guild.GuildMembers.FirstOrDefault(x => x.Id == Context.User.Id);

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

        [Command("warcheck")]
        public async Task WarCheckAsync()
        {
            var result = await Utilites.CalculateWarWinnerAsync(Clash, Guild.ClanTag);

            switch (result)
            {
                case LottoDraw lottoDraw:
                    var highSync = Start.GetSync();
                    var highTagIsClanTag = lottoDraw.HighSyncWinnerTag == lottoDraw.ClanTag;
                    var highSyncWinner = highTagIsClanTag ? lottoDraw.ClanName : lottoDraw.OpponentName;
                    var lowSyncWinner = highTagIsClanTag ? lottoDraw.OpponentName : lottoDraw.ClanName;
                    var sync = highSync ? "high" : "low";
                    var winner = highSync ? highSyncWinner : lowSyncWinner;

                    await SendMessageAsync($"It is a {sync}-sync war and {winner} wins!");
                    break;

                case LottoFailed lottoFailed:
                    await SendMessageAsync(lottoFailed.Reason);
                    break;

                case LottoResult lottoResult:
                    var lottoWinner = lottoResult.ClanWin
                        ? $"{lottoResult.ClanName}"
                        : $"{lottoResult.OpponentName}";

                    await SendMessageAsync($"It is {lottoWinner}'s win!");
                    break;
            }
        }

        [Command("members")]
        public async Task GetMembersAsync()
        {
            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);
            var currentWar = await Clash.GetCurrentWarAsync(Guild.ClanTag);
            var warMembers = currentWar.Clan.Members;

            var membersByDonations = clanMembers.OrderByDescending(x => x.Donations);

            var i = 1;

            var donationList = string.Join('\n', membersByDonations.Select(x => $"{i++}:{x.Name} - **{x.Donations}**"));

            var missedAttackers = warMembers.Where(x => x.Attacks.Count == 0);

            var missedList = string.Join('\n', missedAttackers.Select(x => x.Name));

            await SendMessageAsync(
                $"__**Members Ordered By Donations**__: \n{donationList}\n\n__**Missed Attackers**__:\n{missedList}");
        }

        [Command("clear", "c")]
        public Task ClearAsync(int count = 5)
            => Message.DeleteMessagesAsync(Context, count + 1);

        [Command("discordcheck")]
        public async Task DiscordCheck()
        {
            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);
            var discordMembers = Guild.GuildMembers;

            var missingMembers = clanMembers.Where(clanMember => discordMembers.Any(discordMember =>
                !discordMember.Tags.Any(x =>
                    string.Equals(x, clanMember.Tag, StringComparison.InvariantCultureIgnoreCase)))).ToArray();

            var missingList = string.Join('\n', missingMembers.Select(x => x.Name));

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
    }
}
