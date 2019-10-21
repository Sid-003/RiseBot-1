using BandWrapper;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClashWrapper;
using Casino.Discord;

#pragma warning disable 4014

namespace RiseBot.Services
{
    [Service]
    public class StartTimeService
    {
        private readonly DiscordSocketClient _client;
        private readonly BandClient _band;
        private readonly ClashClient _clash;
        private readonly DatabaseService _database;

        //TODO prob move this to config
        private readonly string BandKey;
        private const string Locale = "en_GB";

        private static TimeSpan Period => TimeSpan.FromMinutes(30);

        private bool? _highSync;
        private IUserMessage _lastMessage;
        private DateTimeOffset _start;
        private DateTimeOffset _end;

        private CancellationTokenSource _tokenSource;

        public StartTimeService(DiscordSocketClient client, BandClient band, DatabaseService database, ClashClient clash)
        {
            _client = client;
            _band = band;
            _database = database;
            _clash = clash;
            _tokenSource = new CancellationTokenSource();

            _client.ReactionAdded += async (cache, channel, reaction) =>
            {
                if (cache.Id != _lastMessage?.Id || reaction.UserId == _client.CurrentUser.Id)
                    return;

                var user = await _client.GetOrFetchUserAsync(reaction.UserId);

                switch (reaction.Emote.Name)
                {
                    case "✅":
                        await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                        {
                            Description = $"{user.Mention} can start war",
                            Color = Color.Green
                        }
                            .Build());

                        break;

                    case "❌":
                        await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                        {
                            Description = $"{user.Mention} cannot start war",
                            Color = Color.Red
                        }
                            .Build());

                        break;

                    case "⚠":
                        await channel.SendMessageAsync(string.Empty, embed: new EmbedBuilder
                        {
                            Description = $"{user.Mention} can maybe start war",
                            Color = Color.Orange
                        }
                            .Build());
                        break;
                }
            };
        }

        //TODO cancel reminders if time is changed
        public async Task StartServiceAsync()
        {
            var lastPostKey = "";
            var lastTimePostKey = "";
            var handledPost = false;

            while (true)
            {
                try
                {
                    await Task.Delay(Period);
                    var posts = await _band.GetPostsAsync(BandKey, Locale, 3);

                    if (posts is null || posts.Count == 0)
                    {
                        handledPost = false;
                        continue;
                    }

                    var lastPost = posts.First();

                    if (lastPostKey == lastPost.Key && handledPost)
                        continue;

                    handledPost = false;

                    lastPostKey = lastPost.Key;
                    var guild = _database.Guild;

                    var post = await _band.GetPostAsync(BandKey, lastPostKey);

                    if (post is null)
                    {
                        handledPost = false;
                        continue;
                    }

                    IList<FWARep> reps;
                    Dictionary<ulong, (DateTimeOffset, DateTimeOffset)> times;

                    if (!(_client.GetChannel(guild.RepChannelId) is SocketTextChannel repChannel))
                        continue;
                    
                    if (post?.Content?.Contains("SYNC TIME HAS BEEN CHANGED", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        _tokenSource.Cancel(true);

                        post = await _band.GetPostAsync(BandKey, lastTimePostKey);
                        _start = post.Schedule.Start;
                        _end = post.Schedule.End;

                        reps = guild.FWAReps;

                        times = reps.ToDictionary(rep => rep.Id,
                            rep => (_start.Add(TimeSpan.FromHours(rep.TimeZone)),
                                _end.Add(TimeSpan.FromHours(rep.TimeZone))));

                        await UpdateLastMessageAsync(times);
                        await repChannel.SendMessageAsync("**@everyone START TIMES HAVE BEEN CHANGED!**");

                        Task.Run(() => RunRemindersAsync(repChannel, guild));

                        continue;
                    }

                    handledPost = true;

                    if (post.Schedule is null)
                        continue;

                    if (!(_client.GetChannel(guild.StartTimeChannelId) is SocketTextChannel startChannel))
                        continue;

                    lastTimePostKey = lastPostKey;
                    _highSync = post.Schedule.Name.Contains("High", StringComparison.InvariantCultureIgnoreCase);

                    _start = post.Schedule.Start;
                    _end = post.Schedule.End;

                    if (_end - _start != TimeSpan.FromHours(2))
                        await repChannel.SendMessageAsync("ding ding ding");

                    reps = guild.FWAReps;

                    times = reps.ToDictionary(rep => rep.Id,
                        rep => (_start.Add(TimeSpan.FromHours(rep.TimeZone)),
                            _end.Add(TimeSpan.FromHours(rep.TimeZone))));

                    var embed = BuildEmbed(times);

                    _lastMessage = await startChannel.SendMessageAsync("@everyone", embed: embed);
                    await _lastMessage.AddReactionsAsync(new[] { new Emoji("✅"), new Emoji("⚠"), new Emoji("❌") });
                    
                    Task.Run(() => RunRemindersAsync(repChannel, guild));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public bool? GetSync()
            => _highSync;

        public Task UpdateLastMessageAsync(Dictionary<ulong, (DateTimeOffset, DateTimeOffset)> times)
        {
            return _lastMessage is null
                ? Task.CompletedTask
                : _lastMessage.ModifyAsync(x => x.Embed = BuildEmbed(times));
        }

        public (DateTimeOffset, DateTimeOffset) GetStartEndTimes()
            => (_start, _end);

        private Embed BuildEmbed(Dictionary<ulong, (DateTimeOffset, DateTimeOffset)> times)
        {
            return new EmbedBuilder
            {
                Title = "Sync Times Posted!",
                Color = new Color(0x10c1f7),
                Description = $"It is a {(_highSync == true ? "high" : "low")}-sync war",
                Timestamp = DateTimeOffset.UtcNow
            }
            .AddField("Sync Times!",
                string.Join('\n',
                    times.Select(x =>
                        $"{_client.GetUser(x.Key).Mention} : **Start**, {x.Value.Item1:t} - **End**,{x.Value.Item2:t}")))
            .AddField("\u200b", "React with ✅ if you can start war, ⚠ if maybe and, ❌ if not")
            .Build();
        }

        private async Task RunRemindersAsync(ISocketMessageChannel repChannel, Guild guild)
        {
            try
            {
                await Task.Delay(_start - DateTimeOffset.UtcNow - TimeSpan.FromHours(1)).ContinueWith(_ =>
                    repChannel.SendMessageAsync("@everyone search is in 1hr!"));
                await Task.Delay(_start - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10)).ContinueWith(
                    _ => repChannel.SendMessageAsync("@everyone search is in 10 minutes!"),
                    _tokenSource.Token);
                await Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(
                    async _ => await repChannel.SendMessageAsync($"@everyone search has started!\n{await Utilites.GetOrderedMembersAsync(_clash, guild.ClanTag)}"),
                    _tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource();
            }
        }
    }
}
