using System;
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

        private bool _highSync = true;

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
                    await Task.Delay(30000);
                    var posts = await _band.GetPostsAsync(BandKey, Locale, 3);

                    if(posts is null)
                        continue;

                    var lastPost = posts.First();

                    if (lastPostKey == lastPost.Key && handledPost)
                        continue;

                    handledPost = false;

                    lastPostKey = lastPost.Key;
                    var guild = _database.Guild;

                    var post = await _band.GetPostAsync(BandKey, lastPostKey);

                    if(post is null)
                        continue;

                    if (!(_client.GetChannel(guild.RepChannelId) is SocketTextChannel repChannel))
                        continue;

                    if (_webhook is null)
                    {
                        var webhooks = await repChannel.GetWebhooksAsync();
                        var bandHook = webhooks.FirstOrDefault(x => x.Name == "Band");

                        _webhook = new DiscordWebhookClient(bandHook);
                    }
                    
                    var builder = new EmbedBuilder
                    {
                        Title = post.Schedule is null ? "New FWA Post!" : post.Schedule.Name,
                        Description = post.Content.Length > 2048 ? post.Content.Substring(0, 2048) : post.Content,
                        ThumbnailUrl = "https://upload.wikimedia.org/wikipedia/commons/3/30/2._BAND_Icon.png",
                        Color = new Color(0x11f711)
                    };

                    if (_webhook is null)
                    {
                        await repChannel.SendMessageAsync(string.Empty, embed: builder.Build());
                    }
                    else
                    {
                        await _webhook.SendMessageAsync(string.Empty, embeds: new[] { builder.Build() });
                    }

                    handledPost = true;

                    if (post.Schedule is null)
                        continue;

                    if (!(_client.GetChannel(guild.StartTimeChannelId) is SocketTextChannel startChannel))
                        continue;

                    _highSync = post.Schedule.Name.Contains("High");

                    var start = post.Schedule.Start;
                    var end = post.Schedule.End;

                    var reps = guild.FWAReps;

                    var times = reps.ToDictionary(rep => rep.Id,
                        rep => (start.Add(TimeSpan.FromHours(rep.TimeZone)),
                            end.Add(TimeSpan.FromHours(rep.TimeZone))));

                    builder = new EmbedBuilder
                    {
                        Title = "Sync Times Posted!",
                        Color = new Color(0x10c1f7),
                        Description = $"It is a {(_highSync ? "high" : "low")}-sync war",
                        Timestamp = DateTimeOffset.UtcNow
                    }
                        .AddField("Sync Times!",
                            string.Join('\n',
                                times.Select(x =>
                                    $"{_client.GetUser(x.Key).Mention} : **Start**, {x.Value.Item1:t} - **End**,{x.Value.Item2:t}")));

                    await startChannel.SendMessageAsync("@everyone", embed: builder.Build());

#pragma warning disable 4014
                    Task.Run(async () =>
                    {
                        await Task.Delay(start - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(10));
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

        public bool GetSync()
            => _highSync;
    }
}
