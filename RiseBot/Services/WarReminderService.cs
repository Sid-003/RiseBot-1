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

            _clash.Error += (message) =>
            {
                if (message.Error != "inMaintenance")
                    return Task.CompletedTask;

                _cancellationTokenSource.Cancel(true);
                _cancellationTokenSource = new CancellationTokenSource();
                return Task.CompletedTask;
            };
        }

        public async Task StartServiceAsync()
        {
            var previousState = WarState.Default;
            var remindersSet = false;

            while (true)
            {
                await Task.Delay(30000);
                var currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                if (previousState == currentWar.State)
                    continue;

                previousState = currentWar.State;

                if (remindersSet)
                    continue;

                remindersSet = true;

                var startTime = currentWar.StartTime;
                var endTime = currentWar.EndTime;

                var cancellationToken = _cancellationTokenSource.Token;

#pragma warning disable 4014
                Task.Run(async () =>
#pragma warning restore 4014
                    {
                        try
                        {
                            await Task.Delay(startTime - DateTimeOffset.UtcNow, cancellationToken);

                            var guild = _database.Guild;
                            var channelId = guild.WarChannelId;

                            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                                return;

                            var result = await Utilites.CalculateWarWinnerAsync(_clash, ClanTag);

                            switch (result)
                            {
                                case LottoDraw lottoDraw:
                                    var highSync = _start.GetSync();
                                    var highTagIsClanTag = lottoDraw.HighSyncWinnerTag == lottoDraw.ClanTag;
                                    var highSyncWinner = highTagIsClanTag ? lottoDraw.ClanName : lottoDraw.OpponentName;
                                    var lowSyncWinner = highTagIsClanTag ? lottoDraw.OpponentName : lottoDraw.ClanName;
                                    var sync = highSync ? "high" : "low";
                                    var winner = highSync ? highSyncWinner : lowSyncWinner;

                                    await channel.SendMessageAsync($"It is a {sync}-sync war and {winner} wins!");
                                    break;

                                case LottoFailed lottoFailed:
                                    await channel.SendMessageAsync(lottoFailed.Reason);
                                    remindersSet = false;
                                    _cancellationTokenSource.Cancel(true);
                                    _cancellationTokenSource = new CancellationTokenSource();
                                    break;

                                case LottoResult lottoResult:
                                    var lottoWinner = lottoResult.ClanWin
                                        ? $"{lottoResult.ClanName}"
                                        : $"{lottoResult.OpponentName}";

                                    await channel.SendMessageAsync($"It is {lottoWinner}'s win!");
                                    break;
                            }

                            await Task.Delay(endTime - DateTimeOffset.UtcNow.AddHours(1), cancellationToken);
                            
                            var guildMembers = guild.GuildMembers;

                            var needToAttack = currentWar.Clan.Members.Where(x => x.Attacks.Count < 2).ToArray();

                            var inDiscord = guildMembers.Where(guildMember =>
                                guildMember.Tags.Any(tag => needToAttack.Any(x => x.Tag == tag))).ToArray();

                            var mentions = string.Join('\n', inDiscord.Select(x => $"{_client.GetUser(x.Id).Mention} you need to attack!"));

                            await channel.SendMessageAsync($"War ends in one hour!\n{mentions}");

                            await Task.Delay(endTime - DateTimeOffset.UtcNow, cancellationToken);

                            var missedAttacks = currentWar.Clan.Members.Where(x => x.Attacks.Count == 2).ToArray();

                            inDiscord = guildMembers.Where(guildMember =>
                                guildMember.Tags.Any(tag => missedAttacks.Any(x => x.Tag == tag))).ToArray();

                            mentions = string.Join('\n', inDiscord.Select(x => $"{_client.GetUser(x.Id).Mention} you missed your attacks!"));

                            await channel.SendMessageAsync($"War has ended!\n{mentions}");
                        }
                        catch (TaskCanceledException)
                        {
                            remindersSet = false;
                            previousState = WarState.Default;
                        }

                    },
                    cancellationToken);
            }
        }
    }
}
