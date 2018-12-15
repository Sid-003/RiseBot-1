using BandWrapper;
using ClashWrapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pusharp;
using Qmmands;
using RiseBot.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RiseBot
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private DatabaseService _database;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            var config = Config.Create("./config.json");

            _database = await new DatabaseService().LoadGuildAsync();

            var clashClient = new ClashClient(new ClashClientConfig
            {
                Token = config.ClashToken
            });

            var bandClient = new BandClient(new BandClientConfig
            {
                Token = config.BandToken
            });

            var pushClient = new PushBulletClient(new PushBulletClientConfig
            {
                LogLevel = LogLevel.Verbose,
                Token = config.PushBulletToken,
                UseCache = true
            });
            
            _services = new ServiceCollection()
                .AddSingleton(_client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true
                }))
                .AddSingleton(_commands = new CommandService(new CommandServiceConfiguration
                {
                    CaseSensitive = false
                })
                    .AddTypeParsers())
                .AddSingleton(_database)
                .AddSingleton(clashClient)
                .AddSingleton(bandClient)
                .AddSingleton(pushClient)
                .AddServices()
                .BuildServiceProvider();

            var tcs = new TaskCompletionSource<Task>();

            _client.Ready += () =>
            {
                tcs.SetResult(Task.CompletedTask);
                return Task.CompletedTask;
            };

            _client.UserJoined += user =>
            {
                Task.Run(async () =>
                {
                    var guild = _database.Guild;
                    var dGuild = _client.GetGuild(guild.Id);

                    var channel = dGuild.GetTextChannel(guild.WelcomeChannelId);
                    var role = dGuild.GetRole(guild.NotVerifiedRoleId);

                    await user.AddRoleAsync(role);

                    var builder = new EmbedBuilder
                    {
                        Color = Color.Gold,
                        ThumbnailUrl = dGuild.CurrentUser.GetAvatarUrl(),
                        Title = "Welcome to the Discord!",
                        Description = $"{user.GetDisplayName()} welcome to Reddit Rise!\n" +
                                      "Please post a picture of your FWA base, " +
                                      "and if you're feeling nice post your in game player tag (e.g. #YRQ2Y0UC) so we know who you are!"
                    };

                    await channel.SendMessageAsync(user.Mention, embed: builder.Build());
                });
                return Task.CompletedTask;
            };

            var logger = _services.GetService<LogService>();
            
            _client.Log += message =>
            {
                var (source, severity, lMessage, exception) = LogFactory.FromDiscord(message);
                return logger.LogAsync(source, severity, lMessage, exception);
            };

            clashClient.Log += message => logger.LogAsync(Source.Clash, Severity.Verbose, message);
            clashClient.Error += error => logger.LogAsync(Source.Clash, Severity.Error, error.Message);

            bandClient.Log += message => logger.LogAsync(Source.Band, Severity.Verbose, message);
            bandClient.Error += error => logger.LogAsync(Source.Band, Severity.Error, error.Message);

            pushClient.Log += message =>
            {
                var (source, severity, lMessage) = LogFactory.FromPusharp(message);
                return logger.LogAsync(source, severity, lMessage);
            };
            
            await pushClient.ConnectAsync();

            await _client.LoginAsync(TokenType.Bot, config.BotToken);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            await tcs.Task;

            _services.GetService<MessageService>();

#if !DEBUG
            Task.Run(() => _services.GetService<WarReminderService>().StartServiceAsync());
            Task.Run(() => _services.GetService<StartTimeService>().StartServiceAsync());
#endif
            //TODO user joined
            await Task.Delay(-1);
        }
    }
}
