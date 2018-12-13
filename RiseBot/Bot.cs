using ClashWrapper;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using RiseBot.Commands;
using RiseBot.Services;
using System;
using System.Net.Http;
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
            var httpClient = new HttpClient();

            _database = await new DatabaseService().InitialiseAsync();

            var clashClient = new ClashClient(new ClashClientConfig
            {
                Token = config.ClashToken,
                HttpClient = httpClient
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
                .AddServices()
                .BuildServiceProvider();

            var tcs = new TaskCompletionSource<Task>();

            _client.Ready += () =>
            {
                tcs.SetResult(Task.CompletedTask);
                return Task.CompletedTask;
            };

            _client.Log += message =>
            {
                Console.WriteLine(message);
                return Task.CompletedTask;
            };

            _client.MessageReceived += HandleMessageAsync;

            await _client.LoginAsync(TokenType.Bot, config.BotToken);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            await tcs.Task;

            Task.Run(() => _services.GetService<WarReminderService>().StartService());

            await Task.Delay(-1);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) ||
                string.IsNullOrWhiteSpace(message.Content)) return;

            if (CommandUtilities.HasPrefix(message.Content, '~', true,
                out var result))
            {
                var res = await _commands.ExecuteAsync(result,
                    new RiseContext(_client, message), _services);
            }
        }
    }
}
