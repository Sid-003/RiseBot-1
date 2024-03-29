﻿using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.TypeParsers
{
    public sealed class SocketGuildChannelParser : TypeParser<SocketTextChannel>
    {
        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter param, string value, CommandContext ctx)
        {
            var context = ctx as RiseContext;
            if (context.Guild == null)
                new TypeParserResult<SocketTextChannel>("This command must be used in a guild.");
            
            IEnumerable<SocketTextChannel> channels = context.Guild.TextChannels;

            SocketTextChannel channel = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '#' && value[value.Length - 1] == '>' && ulong.TryParse(value.Substring(2, value.Length - 3), out var id)
                || ulong.TryParse(value, out id))
                channel = channels.FirstOrDefault(x => x.Id == id);

            if (channel == null)
                channel = channels.FirstOrDefault(x => x.Name == value);

            return channel == null
                ? new TypeParserResult<SocketTextChannel>("No channel found matching the input.")
                : new TypeParserResult<SocketTextChannel>(channel);
        }
    }
}
