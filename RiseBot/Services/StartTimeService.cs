using BandWrapper;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 4014

namespace RiseBot.Services
{
    [Service]
    public class StartTimeService
    {
        private readonly DiscordSocketClient _client;
        private readonly BandClient _band;
        private readonly DatabaseService _database;
        private DiscordWebhookClient _webhook;

        //TODO prob move this to config
        private const string BandKey = "AADMPvOeSi6era-iwqaVkEtP";
        private const string Locale = "en_GB";

        private static TimeSpan Period => TimeSpan.FromMinutes(30);

        private bool? _highSync;
        private IUserMessage _lastMessage;
        private DateTimeOffset _start;
        private DateTimeOffset _end;

        private CancellationTokenSource _tokenSource;

        public StartTimeService(DiscordSocketClient client, BandClient band, DatabaseService database)
        {
            _client = client;
            _band = band;
            _database = database;
            _tokenSource = new CancellationTokenSource();
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

                    if (post.Content.Contains("SYNC TIME HAS BEEN CHANGED", StringComparison.InvariantCultureIgnoreCase))
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
                        await repChannel.SendMessageAsync("**<@&356176442716848132> START TIMES HAVE BEEN CHANGED!**");

                        Task.Run(() => RunRemindersAsync(repChannel));

                        continue;
                    }

                    //no one actually uses this, too lazy to get rid of it
                    if (_webhook is null)
                    {
                        var webhooks = await repChannel.GetWebhooksAsync();
                        var bandHook = webhooks.FirstOrDefault(x => x.Name == "Band");

                        _webhook = new DiscordWebhookClient(bandHook);
                    }

                    //var builder = new EmbedBuilder
                    //{
                    //Title = post.Schedule is null ? "New FWA Post!" : post.Schedule.Name,
                    //Description = post.Content.Length > 2048 ? post.Content.Substring(0, 2048) : post.Content,
                    //ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/3/30/2._BAND_Icon.png",
                    //Color = new Color(0x11f711)
                    //};

                    if (_webhook is null)
                    {
                        //await repChannel.SendMessageAsync(string.Empty, embed: builder.Build());
                        await repChannel.SendMessageAsync("There is a new post in the sync band");
                    }
                    else
                    {
                        //await _webhook.SendMessageAsync(string.Empty, embeds: new[] { builder.Build() });
                        await _webhook.SendMessageAsync("There is a new post in the sync band");
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

                    reps = guild.FWAReps;

                    times = reps.ToDictionary(rep => rep.Id,
                        rep => (_start.Add(TimeSpan.FromHours(rep.TimeZone)),
                            _end.Add(TimeSpan.FromHours(rep.TimeZone))));

                    var embed = BuildEmbed(times);

                    _lastMessage = await startChannel.SendMessageAsync("@everyone", embed: embed);
                    
                    Task.Run(() => RunRemindersAsync(repChannel));
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
                        $"{_client.GetUser(x.Key).Mention} : **Start**, {x.Value.Item1:t} - **End**,{x.Value.Item2:t}"))).Build();
        }

        private async Task RunRemindersAsync(ISocketMessageChannel repChannel)
        {
            try
            {
                //TODO rep role in db
                //why did I make this ugly
                await Task.Delay(_start - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10)).ContinueWith(
                    _ => repChannel.SendMessageAsync("<@&356176442716848132> search is in 10 minutes!"),
                    _tokenSource.Token);
                await Task.Delay(_start - DateTimeOffset.UtcNow).ContinueWith(
                    _ => repChannel.SendMessageAsync("<@&356176442716848132> seasrch has started!"),
                    _tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                _tokenSource = new CancellationTokenSource();
            }
        }
    }
}
