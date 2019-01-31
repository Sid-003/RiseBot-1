using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands;
using RiseBot.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Services
{
    [Service]
    public class MessageService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly DatabaseService _database;
        private readonly LogService _logger;
        private readonly TimerService _timer;
        private readonly IServiceProvider _services;

        private readonly
            ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string, CachedMessage>>>
            _messageCache;

        private static TimeSpan MessageLifeTime => TimeSpan.FromMinutes(10);

        private DiscordWebhookClient _webhook;
        
        public MessageService(CommandService commands, DiscordSocketClient client, DatabaseService database,
            LogService logger, TimerService timer, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _database = database;
            _logger = logger;
            _timer = timer;
            _services = services;

            _messageCache =
                new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, ConcurrentDictionary<string,
                    CachedMessage>>>();

            _commands.CommandErrored += CommandErroredAsync;
            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += (msg) =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, false) : Task.CompletedTask;
            _client.MessageUpdated += (_, msg, __) =>
                msg is SocketUserMessage message ? HandleReceivedMessageAsync(message, true) : Task.CompletedTask;

            _client.MessageDeleted += HandleDeletedAsync;
        }

        //TODO not shit
        private async Task HandleDeletedAsync(Cacheable<IMessage, ulong> cacheable, ISocketMessageChannel _)
        {
            var message = await cacheable.GetOrDownloadAsync();

            var manda = _client.GetUser(241828624787832833);

            if (message is null || message.Author.Id != manda.Id)
                return;

            var channel = (SocketTextChannel) _client.GetChannel(530739486573854741);

            if (_webhook is null)
            {
                var webhooks = await channel.GetWebhooksAsync();
                _webhook = new DiscordWebhookClient(webhooks.First());
            }

            await _webhook.SendMessageAsync(message.Content);
        }

        private async Task HandleReceivedMessageAsync(SocketUserMessage message, bool isEdit)
        {
            if (message.Channel is IPrivateChannel) return;

            var context = new RiseContext(_client, message, isEdit);
            var guild = _database.Guild;
            
            var prefix = guild.Prefix;

            if (CommandUtilities.HasPrefix(message.Content, prefix, out var output) || 
                message.HasMentionPrefix(_client.CurrentUser, out output))
            {
                var result = await _commands.ExecuteAsync(output, context, _services);

                if (result is CommandNotFoundResult)
                {
                    var emote = Emote.Parse("<:blobcatgooglythink:537634878250811403>");
                    await message.AddReactionAsync(emote);
                }
                else if (!result.IsSuccessful)
                {
                    var failed = result as FailedResult;

                    await SendMessageAsync(context, failed?.Reason);
                }
            }
        }
        
        private async Task CommandErroredAsync(ExecutionFailedResult result, ICommandContext originalContext, IServiceProvider services)
        {
            if (!(originalContext is RiseContext context))
                return;

            await SendMessageAsync(context, result.Reason);

            if (!string.IsNullOrWhiteSpace(result.Exception.ToString()))
                await _logger.LogAsync(Source.Commands, Severity.Error, string.Empty, result.Exception);
        }

        private async Task CommandExecutedAsync(Command command, CommandResult originalResult,
            ICommandContext originalContext, IServiceProvider services)
        {
            if (!(originalContext is RiseContext context))
                return;

            await _logger.LogAsync(Source.Commands, Severity.Verbose,
                $"Successfully executed {{{command.Name}}} for {{{context.User.GetDisplayName()}}} in " +
                $"{{{context.Guild.Name}/{context.Channel.Name}}}");
        }
        
        public async Task<IUserMessage> SendMessageAsync(RiseContext context, string content, Embed embed = null)
        {
            if (!_messageCache.TryGetValue(context.Channel.Id, out var foundChannel))
                foundChannel = (_messageCache[context.Channel.Id] =
                    new ConcurrentDictionary<ulong, ConcurrentDictionary<string, CachedMessage>>());

            if (!foundChannel.TryGetValue(context.User.Id, out var foundCache))
                foundCache = (foundChannel[context.User.Id] = new ConcurrentDictionary<string, CachedMessage>());

            var foundMessage = foundCache.FirstOrDefault(x => x.Value.ExecutingId == context.Message.Id);

            if (context.IsEdit && !foundMessage.Equals(default(KeyValuePair<string, CachedMessage>)))
            {
                if (await GetOrDownloadMessageAsync(foundMessage.Value.ChannelId, foundMessage.Value.ResponseId) is
                    IUserMessage fetchedMessage)
                {
                    await fetchedMessage.ModifyAsync(x =>
                    {
                        x.Content = content;
                        x.Embed = embed;
                    });

                    return fetchedMessage;
                }
            }

            var sentMessage = await context.Channel.SendMessageAsync(content, embed: embed);

            var message = new CachedMessage
            {
                ChannelId = context.Channel.Id,
                ExecutingId = context.Message.Id,
                UserId = context.User.Id,
                ResponseId = sentMessage.Id,
                CreatedAt = sentMessage.CreatedAt.ToUnixTimeMilliseconds()
            };

            var when = DateTimeOffset.UtcNow.Add(MessageLifeTime).ToUnixTimeMilliseconds();
            var key = await _timer.EnqueueAsync(message, when, RemoveAsync);

            _messageCache[context.Channel.Id][context.User.Id][key] = message;

            return sentMessage;
        }

        private Task<IMessage> GetOrDownloadMessageAsync(ulong channelId, ulong messageId)
        {
            if (!(_client.GetChannel(channelId) is SocketTextChannel channel))
                return null;

            return !(channel.GetCachedMessage(messageId) is IMessage message)
                ? channel.GetMessageAsync(messageId)
                : Task.FromResult(message);
        }

        private Task RemoveAsync(string key, object removable)
        {
            var message = removable as CachedMessage;
            _messageCache[message.ChannelId][message.UserId].TryRemove(key, out _);

            if (_messageCache[message.ChannelId][message.UserId].Count == 0)
                _messageCache.Remove(message.UserId, out _);

            if (_messageCache[message.ChannelId].Count == 0)
                _messageCache.Remove(message.ChannelId, out _);

            return Task.CompletedTask;
        }

        public async Task DeleteMessagesAsync(RiseContext context, int amount)
        {
            var perms = context.Guild.CurrentUser.GetPermissions(context.Channel);
            var manageMessages = perms.ManageMessages;

            var deleted = 0;

            do
            {
                if (!_messageCache.TryGetValue(context.Channel.Id, out var foundCache))
                    return;

                if (!foundCache.TryGetValue(context.User.Id, out var found))
                    return;

                if (found is null)
                    return;

                if (found.Count == 0)
                {
                    _messageCache[context.Channel.Id].Remove(context.User.Id, out _);

                    if (_messageCache[context.Channel.Id].Count == 0)
                        _messageCache.Remove(context.Channel.Id, out _);

                    return;
                }

                var ordered = found.OrderByDescending(x => x.Value.CreatedAt).ToImmutableArray();
                amount = amount > ordered.Length ? ordered.Length : amount;

                var toDelete = new List<(string, CachedMessage)>();

                for (var i = 0; i < amount; i++)
                    toDelete.Add((ordered[i].Key, ordered[i].Value));

                var res = await DeleteMessagesAsync(context, manageMessages, toDelete);
                deleted += res;

            } while (deleted < amount);
        }

        private async Task<int> DeleteMessagesAsync(RiseContext context, bool manageMessages,
            IEnumerable<(string Key, CachedMessage Cached)> messages)
        {
            var fetchedMessages = new List<IMessage>();

            foreach (var (key, cached) in messages)
            {
                await RemoveAsync(key, cached);
                await _timer.RemoveAsync(key);

                if (await GetOrDownloadMessageAsync(cached.ChannelId, cached.ResponseId) is IMessage fetchedMessage)
                    fetchedMessages.Add(fetchedMessage);
            }

            if (manageMessages)
            {
                await context.Channel.DeleteMessagesAsync(fetchedMessages);
            }
            else
            {
                foreach (var message in fetchedMessages)
                    await context.Channel.DeleteMessageAsync(message);
            }

            return fetchedMessages.Count;
        }
    }
}
