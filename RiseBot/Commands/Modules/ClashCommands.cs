using ClashWrapper;
using ClashWrapper.Entities.WarLog;
using Discord;
using Discord.WebSocket;
using Qmmands;
using RiseBot.Results;
using RiseBot.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    public class ClashCommands : RiseBase
    {
        public ClashClient Clash { get; set; }
        public StartTimeService Start { get; set; }

        [Command("warcheck")]
        [RunMode(RunMode.Parallel)]
        public async Task WarCheckAsync()
        {
            var result = await Utilites.CalculateWarWinnerAsync(Clash, Guild.ClanTag);

            switch (result)
            {
                case LottoDraw lottoDraw:
                    var highSync = Start.GetSync();

                    if (!highSync.HasValue)
                    {
                        await SendMessageAsync($"```css\nIt's a draw but I don't know what sync it is :(\n{result.WarLogComparison}```");
                        return;
                    }

                    var highTagIsClanTag = lottoDraw.HighSyncWinnerTag == lottoDraw.ClanTag;
                    var highSyncWinner = highTagIsClanTag ? lottoDraw.ClanName : lottoDraw.OpponentName;
                    var lowSyncWinner = highTagIsClanTag ? lottoDraw.OpponentName : lottoDraw.ClanName;
                    var sync = highSync == true ? "high" : "low";
                    var winner = highSync == true ? highSyncWinner : lowSyncWinner;

                    await SendMessageAsync($"```css\nIt is a {sync}-sync war and {winner} wins!\n{result.WarLogComparison}```");
                    return;

                case LottoFailed lottoFailed:
                    await SendMessageAsync(lottoFailed.Reason);
                    return;

                case LottoResult lottoResult:
                    var lottoWinner = lottoResult.ClanWin
                        ? $"{lottoResult.ClanName}"
                        : $"{lottoResult.OpponentName}";

                    await SendMessageAsync($"```css\nIt is {lottoWinner}'s win!\n{result.WarLogComparison}```");
                    return;
            }
        }

        [Command("members")]
        [RunMode(RunMode.Parallel)]
        public async Task GetMembersAsync()
        {
            var s = await Utilites.GetOrderedMembersAsync(Clash, Guild.ClanTag);

            await SendMessageAsync(s == "" ? "API is currently under maintenance" : s);
        }

        [Command("warlog")]
        [RunMode(RunMode.Parallel)]
        public async Task GetWarLogAsync()
        {
            var pagedWarLog = await Clash.GetWarLogAsync(Guild.ClanTag, 10);
            var warLog = pagedWarLog.Entity;

            var trimmedLog = Utilites.TrimWarLog(warLog);

            var sb = new StringBuilder();
            sb.AppendLine("Reddit Rise's Last 7 Wars");
            sb.AppendLine();

            foreach (var war in trimmedLog)
            {
                sb.AppendLine(war.Result == WarResult.Lose ? $"- [LOSS] {war.Opponent.Name}" : $"+ [WIN ] {war.Opponent.Name}");
            }

            await SendMessageAsync($"```diff\n{sb}\n```");
        }

        [Command("missed")]
        public Task GetMissedAsync([Remainder] SocketGuildUser user)
        {
            var members = Guild.GuildMembers;
            var found = members.FirstOrDefault(x => x.Id == user.Id); //TODO move into typereader

            return SendMessageAsync(found is null
                ? "This user doesn't have an account in the clan"
                : $"This user has missed both attacks in {found.MissedAttacks} out of {found.TotalWars} wars");
        }

        [Command("missed")]
        public async Task ViewFrequentMissersAsync()
        {
            var guildMembers = Guild.GuildMembers;

            var clanMembers = await Clash.GetClanMembersAsync(Guild.ClanTag);

            var combined = (from clanMember in clanMembers
                let foundGuildMember =
                    guildMembers.FirstOrDefault(guildMember => guildMember.Tags.Any(tag =>
                        string.Equals(tag, clanMember.Tag, StringComparison.InvariantCultureIgnoreCase)))
                where !(foundGuildMember is null)
                select (clanMember, foundGuildMember)).ToArray();

            var frequents = combined.Where(x =>
                (float) x.foundGuildMember.MissedAttacks / x.foundGuildMember.TotalWars > 0.5);

            var message = string.Join('\n',
                frequents.Select(x =>
                    $"{x.clanMember.Name}{x.clanMember.Tag} {x.foundGuildMember.MissedAttacks}/{x.foundGuildMember.TotalWars}"));

            await SendMessageAsync($"__**People with >50% missed attacks**__\n{message}");
        }

        [Command("profile")]
        public async Task ViewProfileAsync(string tag)
        {
            var player = await Clash.GetPlayerAsync(tag);

            if(player is null)
            {
                await SendMessageAsync("Failed to find player.");
                return;
            }

            var builder = new EmbedBuilder
            {
                Title = player.Name + player.Tag,
                Url = $"https://link.clashofclans.com/en?action=OpenPlayerProfile&tag={player.Tag.Replace("#", "")}",
                Color = Color.Blue,
                Description = $"Townhall: {player.TownHallLevel}"
            };

            builder.AddField("Heroes", string.Join("\n", player.Heroes.Where(x => x.Village == "home").Select(x => $"{x.Name}: **{x.Level}**")));
            //builder.AddField("Troops", string.Join("\n", player.Troops.Where(x => x.Village == "home").Select(x => $"{x.Name}: **{x.Level}**")));

            await SendMessageAsync(string.Empty, builder.Build());
        }

        [Command("fwabases")]
        public Task SendFWABasesAsync()
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Gold,
                Description = "Here are some already perfect FWA bases for you. Just click the link that corresponds to your TH.\n" +
                $"{Format.Url("TH12", "https://link.clashofclans.com/en?action=OpenLayout&id=TH12%3AWB%3AAAAAHQAAAAFw3gmOJJOUocokY9SNAt9V")}\n" +
                $"{Format.Url("TH11", "https://link.clashofclans.com/en?action=OpenLayout&id=TH11%3AWB%3AAAAAOwAAAAE4a6sCQApcIa9kDl5W1N3C")}\n" +
                $"{Format.Url("TH10", "https://link.clashofclans.com/en?action=OpenLayout&id=TH10%3AWB%3AAAAAFgAAAAF-L9A_pnLR3BtoRk7SZjD_")}\n" +
                $"{Format.Url("TH9", "https://link.clashofclans.com/en?action=OpenLayout&id=TH9%3AWB%3AAAAAHQAAAAFw3chc3wBw2ipMxGm6Mq8P")}"
            };

            return SendMessageAsync(string.Empty, embed.Build());
        }
    }
}
