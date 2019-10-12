using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord;
using Discord.WebSocket;
using RiseBot.Results;
using System;
using System.Collections.Generic;
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

        private CancellationTokenSource _maintenanceCts;
        private CancellationTokenSource _noMaintenanceCts;

        private TaskCompletionSource<bool> _warTcs;
        private bool _isWin;

        private const string ClanTag = "#2GGCRC90";

        public WarReminderService(DiscordSocketClient client, ClashClient clash, DatabaseService database,
            StartTimeService start, LogService logger)
        {
            _client = client;
            _clash = clash;
            _database = database;
            _start = start;
            _logger = logger;

            _maintenanceCts = new CancellationTokenSource();
            _noMaintenanceCts = new CancellationTokenSource();

            _warTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _clash.Error += message =>
            {
                if (message.Reason != "inMaintenance")
                    return Task.CompletedTask;

                if (!_maintenanceCts.IsCancellationRequested)
                {
                    _maintenanceCts.Cancel(true);
                    _maintenanceCts.Dispose();
                    _maintenanceCts = new CancellationTokenSource();
                }

                return Task.CompletedTask;
            };
        }

        public async Task StartRemindersAsync()
        {
            var dbGuild = _database.Guild;

            var channel = _client.GetChannel(dbGuild.WarChannelId) as SocketTextChannel;
            var guild = channel.Guild;

            IRole warRole = null;

            while (true)
            {
                try
                {
                    var currentWar = await _clash.GetCurrentWarAsync(dbGuild.ClanTag);

                    if (currentWar is null || currentWar.State == WarState.Default || currentWar.State == WarState.Ended)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(10), _maintenanceCts.Token);
                        continue;
                    }

                    async Task RunRemindersAsync(bool hasMatched)
                    {
                        TimeSpan threshold;

                        if(hasMatched)
                        {
                            currentWar = await _clash.GetCurrentWarAsync(dbGuild.ClanTag);
                            threshold = DateTimeOffset.UtcNow - currentWar.StartTime;
                        }

                        else
                            threshold = DateTimeOffset.UtcNow - currentWar.PreparationTime;

                        if(threshold < TimeSpan.FromMinutes(60))
                        {
                            if(!hasMatched)
                            {
                                warRole = await WarMatchAsync(channel, dbGuild, guild, currentWar);

                                var startTime = currentWar.StartTime - DateTimeOffset.UtcNow;

                                await Task.Delay(startTime, _maintenanceCts.Token);
                            }

                            await channel.SendMessageAsync($"{warRole?.Mention} war has started!");

                            _warTcs.SetResult(true);

                            var beforeEnd = currentWar.EndTime - DateTimeOffset.UtcNow.AddHours(1);

                            await Task.Delay(beforeEnd, _maintenanceCts.Token);

                            currentWar = await _clash.GetCurrentWarAsync(dbGuild.ClanTag);

                            await NeedToAttackAsync(channel, dbGuild.GuildMembers, currentWar);

                            await Task.Delay(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(10)));

                            currentWar = await _clash.GetCurrentWarAsync(dbGuild.ClanTag);

                            await WarEndedAsync(channel, dbGuild.GuildMembers, currentWar);

                            await warRole.DeleteAsync();
                        }
                    }

                    switch (currentWar.State)
                    {
                        case WarState.Preparation:
                            await RunRemindersAsync(false);
                            break;

                        case WarState.InWar:
                            await RunRemindersAsync(true);
                            break;
                    }

                    await Task.Delay(TimeSpan.FromMinutes(10), _maintenanceCts.Token);
                }
                catch (TaskCanceledException)
                {
                    await channel.SendMessageAsync("Maintenance break");

                    try
                    {
                        await Task.Delay(-1, _noMaintenanceCts.Token);
                    }
                    catch (TaskCanceledException) { }

                    await channel.SendMessageAsync("Maintenance ended");
                }
                catch (Exception ex)
                {
                    await _logger.LogAsync(Source.Reminder, Severity.Error, string.Empty, ex);
                }
                finally
                {
                    _warTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }
        }

        public async Task StartPollingServiceAsync()
        {
            while (true)
            {
                try
                {
                    var res = await _clash.GetCurrentWarAsync(ClanTag);

                    if (res != null)
                    {
                        _noMaintenanceCts.Cancel(true);
                        _noMaintenanceCts.Dispose();
                        _noMaintenanceCts = new CancellationTokenSource();
                    }

                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
                catch(Exception ex)
                {
                    await _logger.LogAsync(Source.Reminder, Severity.Error, string.Empty, ex);
                }
            }
        }

        private async Task<IRole> WarMatchAsync(SocketTextChannel channel, Guild guild, SocketGuild discordGuild, CurrentWar currentWar)
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
                    var sync = highSync == true ? "high" : "low";
                    var win = highSync == true ? highTagIsClanTag ? "wins" : "loses" : highTagIsClanTag ? "loses" : "wins";

                    _isWin = highSync == true && highTagIsClanTag;

                    await channel.SendMessageAsync(
                        $"```css\nIt is a {sync}-sync war and Reddit Rise {win}!\n{result.WarLogComparison}```");
                    break;

                case LottoFailed lottoFailed:
                    await channel.SendMessageAsync(lottoFailed.Reason);

                    _maintenanceCts.Cancel(true);
                    _maintenanceCts.Dispose();
                    _maintenanceCts = new CancellationTokenSource();
                    break;

                case LottoResult lottoResult:
                    var lottoWinner = lottoResult.ClanWin
                        ? $"{lottoResult.ClanName}"
                        : $"{lottoResult.OpponentName}";

                    await channel.SendMessageAsync(
                        $"```css\nIt is {lottoWinner}'s win!\n{result.WarLogComparison}```");
                    break;
            }

            var warRole = await discordGuild.CreateRoleAsync("InWar");
            await warRole.ModifyAsync(x => x.Mentionable = true);

            var inWar = currentWar.Clan.Members;
            var guildMembers = guild.GuildMembers;
            var inDiscord = guildMembers.Where(guildMember =>
                guildMember.Tags.Any(tag => inWar.Any(x => x.Tag == tag))).ToArray();

            foreach (var found in inDiscord.Select(x => discordGuild.GetUser(x.Id)).Where(y => !(y is null)).ToArray())
            {
                if (found.Roles.Any(x => x.Id != guild.NoNotifsRoleId))
                   _ = found.AddRoleAsync(warRole);
            }

            return warRole;
        }

        private async Task NeedToAttackAsync(ISocketMessageChannel channel, IEnumerable<GuildMember> guildMembers, CurrentWar currentWar)
        {
            var needToAttack = currentWar.Clan.Members.Where(x => x.Attacks.Count < 2).ToArray();

            var inDiscord = guildMembers.Where(guildMember =>
                guildMember.Tags.Any(tag => needToAttack.Any(x => x.Tag == tag))).ToArray();

            var mentions = string.Join('\n',
                inDiscord.Select(x =>
                    $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"} you need to attack!"));

            await channel.SendMessageAsync($"War ends in one hour!\n{mentions}");
        }

        private async Task WarEndedAsync(SocketTextChannel channel, IList<GuildMember> guildMembers, CurrentWar currentWar)
        {
            var inDiscord = guildMembers.Where(guildMember =>
                        guildMember.Tags.Any(tag => currentWar.Clan.Members.Any(x =>
                            string.Equals(x.Tag, tag, StringComparison.InvariantCultureIgnoreCase)))).ToArray();

            foreach (var member in inDiscord)
                member.TotalWars++;

            var missedAttacks = currentWar.Clan.Members.Where(x => x.Attacks.Count == 0).ToArray();

            inDiscord = guildMembers.Where(guildMember =>
                guildMember.Tags.Any(tag => missedAttacks.Any(x => x.Tag == tag))).ToArray();

            foreach (var member in inDiscord)
                member.MissedAttacks++;

            _database.UpdateGuild();

            var opponents = currentWar.Opponent.Members.ToDictionary(x => x.Tag, x => x);

            var notMirrored = currentWar.Clan.Members.Where(x =>
                x.Attacks.All(y => opponents[y.DefenderTag].MapPosition != x.MapPosition)).ToArray();

            var names = string.Join('\n',
                inDiscord.Select(x => $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"}"));

            var builder = new EmbedBuilder
            {
                Title = "War Breakdown",
                Color = new Color(0x21a9ff)
            }
                .AddField("Missed Attacks", string.IsNullOrWhiteSpace(names) ? "None :D" : names);

            if (notMirrored.Length > 0)
            {
                builder.AddField("Didn't Attack Mirror",
                    string.Join('\n', notMirrored.Select(x => $"{x.Name}")));
            }

            await channel.SendMessageAsync(string.Join(' ',
                    inDiscord.Select(x => $"{_client.GetUser(x.Id)?.Mention ?? "{User Not Found}"}")),
                embed: builder.Build());
        }

        public async Task WaitTillWarAsync()
        {
            await _warTcs.Task;
        }

        public bool IsWin()
            => _isWin;
    }
}
