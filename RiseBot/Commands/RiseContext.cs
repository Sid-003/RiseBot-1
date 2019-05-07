﻿using Discord.WebSocket;
using Qmmands;

namespace RiseBot.Commands
{
    public class RiseContext : CommandContext
    {
        public DiscordSocketClient Client { get; }
        public SocketUserMessage Message { get; }
        public SocketGuild Guild { get; }
        public SocketGuildUser User { get; }
        public SocketTextChannel Channel { get; }

        public bool IsEdit { get; }

        public RiseContext(DiscordSocketClient client, SocketUserMessage message, bool isEdit = true)
        {
            Client = client;
            Message = message;
            Guild = (message.Channel as SocketGuildChannel)?.Guild;
            User = message.Author as SocketGuildUser;
            Channel = message.Channel as SocketTextChannel;

            IsEdit = isEdit;
        }
    }
}
