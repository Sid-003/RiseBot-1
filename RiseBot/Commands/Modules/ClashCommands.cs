﻿using ClashWrapper;
using Qmmands;
using RiseBot.Results;
using RiseBot.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashWrapper.Entities.WarLog;
using Discord.WebSocket;

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
                        break;
                    }

                    var highTagIsClanTag = lottoDraw.HighSyncWinnerTag == lottoDraw.ClanTag;
                    var highSyncWinner = highTagIsClanTag ? lottoDraw.ClanName : lottoDraw.OpponentName;
                    var lowSyncWinner = highTagIsClanTag ? lottoDraw.OpponentName : lottoDraw.ClanName;
                    var sync = highSync == true ? "high" : "low";
                    var winner = highSync == true ? highSyncWinner : lowSyncWinner;

                    await SendMessageAsync($"```css\nIt is a {sync}-sync war and {winner} wins!\n{result.WarLogComparison}```");
                    break;

                case LottoFailed lottoFailed:
                    await SendMessageAsync(lottoFailed.Reason);
                    break;

                case LottoResult lottoResult:
                    var lottoWinner = lottoResult.ClanWin
                        ? $"{lottoResult.ClanName}"
                        : $"{lottoResult.OpponentName}";

                    await SendMessageAsync($"```css\nIt is {lottoWinner}'s win!\n{result.WarLogComparison}```");
                    break;
            }
        }

        [Command("members")]
        [RunMode(RunMode.Parallel)]
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
            var found = members.FirstOrDefault(x => x.Id == user.Id);

            return SendMessageAsync(found is null
                ? "This user doesn't have an account in the clan"
                : $"This user has missed: {found.MissedAttacks} attacks in {found.TotalWars} wars");
        }
    }
}
