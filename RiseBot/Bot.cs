using BandWrapper;
using Casino.Common;
using Casino.DependencyInjection;
using Casino.Discord;
using Casino.Qmmands;
using ClashWrapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync()
        {
            var config = Config.Create("./config.json");

            var clashClient = new ClashClient(new ClashClientConfig
            {
                Token = config.ClashToken
            });

            var bandClient = new BandClient(new BandClientConfig
            {
                Token = config.BandToken
            });

            var asm = Assembly.GetEntryAssembly();

            _services = new ServiceCollection()
                .AddSingleton(_client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100
                }))
                .AddSingleton(_commands = new CommandService(new CommandServiceConfiguration
                {
                    StringComparison = StringComparison.InvariantCultureIgnoreCase
                })
                    .AddTypeParsers(asm))
                .AddSingleton(clashClient)
                .AddSingleton(bandClient)
                .AddSingleton<TaskQueue>()
                .AddServices(asm.FindTypesWithAttribute<ServiceAttribute>())
                .BuildServiceProvider();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            Task OnReadyAsync()
            {
                tcs.SetResult(true);
                _client.Ready -= OnReadyAsync;
                return Task.CompletedTask;
            }

            _client.Ready += OnReadyAsync;

            _client.UserJoined += user =>
            {
                Task.Run(async () =>
                {
                    var guild = _services.GetService<DatabaseService>().Guild;
                    var dGuild = _client.GetGuild(guild.Id);

                    var channel = dGuild.GetTextChannel(guild.WelcomeChannelId);
                    var role = dGuild.GetRole(guild.NotVerifiedRoleId);

                    await user.AddRoleAsync(role);

                    var builder = new EmbedBuilder
                    {
                        Color = new Color(0x11f711),
                        ThumbnailUrl = dGuild.CurrentUser.GetAvatarUrl(),
                        Title = "Welcome to the Discord!",
                        Description = $"{user.GetDisplayName()} welcome to Reddit Rise!\nHere are some premade, perfect FWA bases. Please click the link that corresponds to your TH.\n" +
                                      $"{Format.Url("TH12", "https://link.clashofclans.com/en?action=OpenLayout&id=TH12%3AWB%3AAAAAHQAAAAFw3gmOJJOUocokY9SNAt9V")}\n" +
                                      $"{Format.Url("TH11", "https://link.clashofclans.com/en?action=OpenLayout&id=TH11%3AWB%3AAAAAOwAAAAE4a6sCQApcIa9kDl5W1N3C")}\n" +
                                      $"{Format.Url("TH10", "https://link.clashofclans.com/en?action=OpenLayout&id=TH10%3AWB%3AAAAAFgAAAAF-L9A_pnLR3BtoRk7SZjD_")}\n" +
                                      $"{Format.Url("TH9", "https://link.clashofclans.com/en?action=OpenLayout&id=TH9%3AWB%3AAAAAHQAAAAFw3chc3wBw2ipMxGm6Mq8P")}\n" +
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

            //TODO do this properly
            _client.UserLeft += (user) =>
            {
                var channel = _client.GetChannel(533650294509404181) as SocketTextChannel;
                return channel.SendMessageAsync($"{user.GetDisplayName()} left");
            };

            clashClient.Log += message => logger.LogAsync(Source.Clash, Severity.Verbose, message);
            clashClient.Error += error => logger.LogAsync(Source.Clash, Severity.Error, error.Message);

            bandClient.Log += message => logger.LogAsync(Source.Band, Severity.Verbose, message);
            bandClient.Error += error => logger.LogAsync(Source.Band, Severity.Error, error.Message);

            await _client.LoginAsync(TokenType.Bot, config.BotToken);
            await _client.StartAsync();

            _commands.AddModules(Assembly.GetEntryAssembly());

            await tcs.Task;

            _services.GetService<MessageService>();

            //Task.Run(() => _services.GetService<BigBrotherService>().RunServiceAsync());
            Task.Run(() => _services.GetService<WarReminderService>().StartRemindersAsync());

#if !DEBUG
            Task.Run(() => _services.GetService<WarReminderService>().StartRemindersAsync());
            Task.Run(() => _services.GetService<WarReminderService>().StartPollingServiceAsync());
            Task.Run(() => _services.GetService<StartTimeService>().StartServiceAsync());
#endif

            await Task.Delay(-1);
        }
    }
}
