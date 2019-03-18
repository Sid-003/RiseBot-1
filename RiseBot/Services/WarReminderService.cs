using ClashWrapper;
using ClashWrapper.Entities.War;
using Discord;
using Discord.WebSocket;
using Pusharp.Entities;
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

                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel(true);
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                return _logger.LogAsync(Source.Reminder, Severity.Verbose, "Maintenance break");
            };
        }

        public async Task StartRemindersAsync()
        {
            var lastState = ReminderState.None;

            CurrentWar currentWar;
            TimeSpan delay;
            var alreadyMatched = false;

            var guild = _database.Guild;
            var channel = _client.GetChannel(guild.WarChannelId) as SocketTextChannel;
            var discordGuild = channel.Guild;

            while (true)
            {
                switch (lastState)
                {
                    case ReminderState.InPrep:

                        currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                        if (currentWar.State == WarState.InWar)
                        {
                            lastState = ReminderState.Start;

                            await channel.SendMessageAsync($"<@&{guild.InWarRoleId}> war has started!");

                            try
                            {
                                delay = currentWar.EndTime - DateTimeOffset.UtcNow.AddHours(1);
                                await Task.Delay(delay, _cancellationTokenSource.Token);
                            }
                            catch (TaskCanceledException)
                            {
                                lastState = ReminderState.Maintenance;
                            }
                        }

                        break;

                    case ReminderState.Start:

                        currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                        await NeedToAttackAsync(channel, guild.GuildMembers, currentWar);

                        lastState = ReminderState.BeforeEnd;

                        try
                        {
                            delay = currentWar.EndTime - DateTimeOffset.UtcNow;
                            await Task.Delay(delay, _cancellationTokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            lastState = ReminderState.Maintenance;
                        }

                        break;

                    case ReminderState.BeforeEnd:

                        currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                        if (currentWar.State == WarState.Ended)
                        {
                            lastState = ReminderState.Ended;
                            alreadyMatched = false;

                            await WarEndedAsync(channel, guild.GuildMembers, currentWar);
                        }

                        break;

                    case ReminderState.None:
                    case ReminderState.Ended:

                        currentWar = await _clash.GetCurrentWarAsync(ClanTag);

                        if (currentWar is null)
                            continue;

                        switch (currentWar.State)
                        {
                            case WarState.Preparation:

                                lastState = ReminderState.InPrep;

                                if(!alreadyMatched)
                                {
                                    await WarMatchAsync(channel, guild, discordGuild, currentWar);

                                    alreadyMatched = true;
                                }

                                try
                                {
                                    delay = currentWar.StartTime - DateTimeOffset.UtcNow;
                                    await Task.Delay(delay, _cancellationTokenSource.Token);
                                }
                                catch (TaskCanceledException)
                                {
                                    lastState = ReminderState.Maintenance;
                                }

                                break;


                            case WarState.InWar:

                                var difference = currentWar.EndTime - DateTimeOffset.UtcNow;

                                lastState = difference > TimeSpan.FromHours(1) 
                                    ? ReminderState.Start
                                    : ReminderState.BeforeEnd;

                                break;
                        }

                        break;

                    case ReminderState.Maintenance:

                        await Task.Delay(TimeSpan.FromSeconds(50));

                        break;
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        public async Task StartPollingServiceAsync()
        {
            while (true)
            {
                await _clash.GetCurrentWarAsync(ClanTag);
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private async Task WarMatchAsync(SocketTextChannel channel, Guild guild, SocketGuild discordGuild, CurrentWar currentWar)
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

            if (!(discordGuild.GetRole(guild.InWarRoleId) is IRole warRole))
            {
                warRole = await discordGuild.CreateRoleAsync("InWar");
                await warRole.ModifyAsync(x => x.Mentionable = true);
                guild.InWarRoleId = warRole.Id;

                _database.UpdateGuild();
            }

            var inWar = currentWar.Clan.Members;
            var guildMembers = guild.GuildMembers;
            var inDiscord = guildMembers.Where(guildMember =>
                guildMember.Tags.Any(tag => inWar.Any(x => x.Tag == tag))).ToArray();

            var foundMembers = inDiscord.Select(x => discordGuild.GetUser(x.Id)).Where(y => !(y is null));

            foreach (var found in foundMembers)
            {
                await found.AddRoleAsync(warRole);
            }
        }

        private async Task NeedToAttackAsync(SocketTextChannel channel, IList<GuildMember> guildMembers, CurrentWar currentWar)
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

            //deleting role because quicker than removing it from everyone
            var discordGuild = _client.GetGuild(_database.Guild.Id);
            var warRole = discordGuild.GetRole(_database.Guild.InWarRoleId);

            await warRole.DeleteAsync();
        }

        private enum ReminderState
        {
            None,
            InPrep,
            Start,
            BeforeEnd,
            Ended,
            Maintenance,
            InWar
        }
    }
}
