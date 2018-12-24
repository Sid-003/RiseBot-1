using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord.WebSocket;
using RiseBot.Results;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class WarReminderService
    {
        private readonly DiscordSocketClient _client;
        private readonly ClashClient _clash;
        private readonly DatabaseService _database;
        private readonly StartTimeService _start;

        private CancellationTokenSource _cancellationTokenSource;

        private const string ClanTag = "#2GGCRC90";

        public WarReminderService(DiscordSocketClient client, ClashClient clash, DatabaseService database, StartTimeService start)
        {
            _client = client;
            _clash = clash;
            _database = database;
            _start = start;

            _cancellationTokenSource = new CancellationTokenSource();

            _clash.Error += (message) =>
            {
                if (message.Reason != "inMaintenance")
                    return Task.CompletedTask;

                _cancellationTokenSource.Cancel(true);
                _cancellationTokenSource = new CancellationTokenSource();
                return Task.CompletedTask;
            };
        }

        public async Task StartServiceAsync()
        {
            var previousState = WarState.Default;

            while (true)
            {
                await Task.Delay(10000);

                var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                if (currentWar is null)
                    continue;

                if (previousState == currentWar.State || currentWar.State != WarState.Preparation)
                    continue;

                previousState = currentWar.State;

                var startTime = currentWar.StartTime;
                var endTime = currentWar.EndTime;

                var cancellationToken = _cancellationTokenSource.Token;

                try
                {
                    var guild = _database.Guild;
                    var channelId = guild.WarChannelId;

                    if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                        return;

                    var result = await Utilites.CalculateWarWinnerAsync(_clash, ClanTag);

                    switch (result)
                    {
                        case LottoDraw lottoDraw:
                            var highSync = _start.GetSync();

                            if (!highSync.HasValue)
                            {
                                await channel.SendMessageAsync(
                                    $"```css\nIt's a draw but I don't know what sync it is :(\n{result.WarLogComparison}```");
                                break;
                            }

                            var highTagIsClanTag = lottoDraw.HighSyncWinnerTag == lottoDraw.ClanTag;
                            var highSyncWinner =
                                highTagIsClanTag ? lottoDraw.ClanName : lottoDraw.OpponentName;
                            var lowSyncWinner =
                                highTagIsClanTag ? lottoDraw.OpponentName : lottoDraw.ClanName;
                            var sync = highSync == true ? "high" : "low";
                            var winner = highSync == true ? highSyncWinner : lowSyncWinner;

                            await channel.SendMessageAsync(
                                $"```css\nIt is a {sync}-sync war and {winner} wins!\n{result.WarLogComparison}```");
                            break;

                        case LottoFailed lottoFailed:
                            await channel.SendMessageAsync(lottoFailed.Reason);
                            
                            _cancellationTokenSource.Cancel(true);
                            _cancellationTokenSource = new CancellationTokenSource();
                            break;

                        case LottoResult lottoResult:
                            var lottoWinner = lottoResult.ClanWin
                                ? $"{lottoResult.ClanName}"
                                : $"{lottoResult.OpponentName}";

                            await channel.SendMessageAsync(
                                $"```css\nIt is {lottoWinner}'s win!\n{result.WarLogComparison}```");
                            break;
                    }

                    var delay = startTime - DateTimeOffset.UtcNow;
                    delay = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;

                    await Task.Delay(delay, cancellationToken);

                    await channel.SendMessageAsync("War has started!");

                    delay = endTime - DateTimeOffset.UtcNow.AddHours(1);
                    delay = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;

                    await Task.Delay(delay, cancellationToken);

                    var guildMembers = guild.GuildMembers;

                    currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                    var needToAttack = currentWar.Clan.Members.Where(x => x.Attacks.Count < 2).ToArray();

                    var inDiscord = guildMembers.Where(guildMember =>
                        guildMember.Tags.Any(tag => needToAttack.Any(x => x.Tag == tag))).ToArray();

                    var mentions = string.Join('\n',
                        inDiscord.Select(x => $"{_client.GetUser(x.Id).Mention} you need to attack!"));

                    await channel.SendMessageAsync($"War ends in one hour!\n{mentions}");

                    delay = endTime - DateTimeOffset.UtcNow;
                    delay = delay < TimeSpan.Zero ? TimeSpan.Zero : delay;

                    await Task.Delay(delay, cancellationToken);

                    currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                    inDiscord = guildMembers.Where(guildMember =>
                        guildMember.Tags.Any(tag => currentWar.Clan.Members.Any(x =>
                            string.Equals(x.Tag, tag, StringComparison.InvariantCultureIgnoreCase)))).ToArray();

                    foreach (var member in inDiscord)
                        member.TotalWars++;

                    var missedAttacks = currentWar.Clan.Members.Where(x => x.Attacks.Count == 0).ToArray();

                    inDiscord = guildMembers.Where(guildMember =>
                        guildMember.Tags.Any(tag => missedAttacks.Any(x => x.Tag == tag))).ToArray();

                    foreach (var member in inDiscord)
                        member.MissedAttacks++;

                    await _database.WriteEntityAsync(guild);

                    mentions = string.Join('\n',
                        inDiscord.Select(x =>
                            $"{_client.GetUser(x.Id).Mention} you missed your attacks! {x.MissedAttacks}/{x.TotalWars}"));

                    await channel.SendMessageAsync($"War has ended!\n{mentions}");
                }
                catch (TaskCanceledException)
                {
                    previousState = WarState.Default;
                }
            }
        }
    }
}
