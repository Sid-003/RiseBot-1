using System;
using System.Collections.Generic;
using BandWrapper;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using Discord.Webhook;

namespace RiseBot.Services
{
    [Service]
    public class StartTimeService
    {
        private readonly DiscordSocketClient _client;
        private readonly BandClient _band;
        private readonly DatabaseService _database;
        private DiscordWebhookClient _webhook;

        private const string BandKey = "AADMPvOeSi6era-iwqaVkEtP";
        private const string Locale = "en_GB";

        private static TimeSpan Period => TimeSpan.FromMinutes(30);

        private bool? _highSync;
        private IUserMessage _lastMessage;
        private DateTimeOffset _start;
        private DateTimeOffset _end;

        public StartTimeService(DiscordSocketClient client, BandClient band, DatabaseService database)
        {
            _client = client;
            _band = band;
            _database = database;
        }

        public async Task StartServiceAsync()
        {
            var lastPostKey = "";
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

                    if (!(_client.GetChannel(guild.RepChannelId) is SocketTextChannel repChannel))
                        continue;

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

                    _highSync = post.Schedule.Name.Contains("High");

                    _start = post.Schedule.Start;
                    _end = post.Schedule.End;

                    var reps = guild.FWAReps;

                    var times = reps.ToDictionary(rep => rep.Id,
                        rep => (_start.Add(TimeSpan.FromHours(rep.TimeZone)),
                            _end.Add(TimeSpan.FromHours(rep.TimeZone))));

                    var embed = BuildEmbed(times);

                    _lastMessage = await startChannel.SendMessageAsync("@everyone", embed: embed);

#pragma warning disable 4014
                    Task.Run(async () =>
                    {
                        await Task.Delay(_start - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10));
                        await repChannel.SendMessageAsync("<@&356176442716848132> search is in 10 minutes!");
                    });
#pragma warning restore 4014
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
    }
}
