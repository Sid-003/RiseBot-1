using Discord.WebSocket;
using Qmmands;
using System;

namespace RiseBot.Commands
{
    public class RiseContext : CommandContext
    {
        public DiscordSocketClient Client { get; }
        public SocketUserMessage Message { get; }
        public SocketGuild Guild { get; }
        public SocketGuildUser User { get; }
        public SocketTextChannel Channel { get; }
        public IServiceProvider Provider { get; }

        public bool IsEdit { get; }

        public RiseContext(DiscordSocketClient client, SocketUserMessage message, IServiceProvider provider, bool isEdit = true) : base(provider)
        {
            Client = client;
            Message = message;
            Guild = (message.Channel as SocketGuildChannel)?.Guild;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;
            Provider = provider;

            IsEdit = isEdit;
        }


    }
}
