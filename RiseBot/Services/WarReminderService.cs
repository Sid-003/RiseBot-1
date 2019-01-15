using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord;
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
        private readonly LogService _logger;

        private CancellationTokenSource _cancellationTokenSource;

        private const string ClanTag = "#2GGCRC90";

        public WarReminderService(DiscordSocketClient client, ClashClient clash, DatabaseService database,
            StartTimeService start, LogService logger)
        {
            _client = client;
            _clash = clash;
            _database = database;
            _start = start;
            _logger = logger;

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

        //TODO think of a way to handle maintenance and still track/remind
        public async Task StartServiceAsync()
        {
            while (true)
            {
                await Task.Delay(10000);

                var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                if (currentWar is null)
                    continue;
                
                if(currentWar.State != WarState.Preparation)
                    continue;
                
                var startTime = currentWar.StartTime;
                var endTime = currentWar.EndTime;

                var cancellationToken = _cancellationTokenSource.Token;

                var guild = _database.Guild;
                var channelId = guild.WarChannelId;

                if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                    return;

                try
                {

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
                        inDiscord.Select(x =>
                            $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"} you need to attack!"));

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

                    var opponents = currentWar.Opponent.Members.ToDictionary(x => x.Tag, x => x);

                    var notMirrored = currentWar.Clan.Members.Where(x =>
                        x.Attacks.Any(y => opponents[y.DefenderTag].MapPosition == x.MapPosition)).ToArray();

                    var builder = new EmbedBuilder
                        {
                            Title = "War Breakdown",
                            Color = new Color(0x21a9ff)
                        }
                        .AddField("Missed Both Attacks",
                            string.Join('\n',
                                inDiscord.Select(x => $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"}")));

                    if (notMirrored.Length > 0)
                    {
                        builder.AddField("Didn't Attack Mirror",
                            string.Join('\n', notMirrored.Select(x => $"{x.Name}{x.Tag}")));
                    }

                    await channel.SendMessageAsync(string.Join(' ',
                            inDiscord.Select(x => $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"}")),
                        embed: builder.Build());
                }
                catch (TaskCanceledException)
                {
                    await channel.SendMessageAsync("Maintenance, reminder cancelled");
                }
                catch (Exception ex)
                {
                    await _logger.LogAsync(Source.Reminder, Severity.Error, ex.Message, ex);
                    await channel.SendMessageAsync("I did an oopsie");
                }
            }
        }
    }
}
