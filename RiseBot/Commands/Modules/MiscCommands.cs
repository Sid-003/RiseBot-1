using ClashWrapper;
using Qmmands;
using RiseBot.Results;
using RiseBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    public class MiscCommands : RiseBase
    {
        public ClashClient Clash { get; set; }
        public StartTimeService Start { get; set; }

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

            await SendMessageAsync($"Members Ordered By Donations:\n{donationList}\n\nMissed Attackers:\n{missedList}");
        }
    }
}
