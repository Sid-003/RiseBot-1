using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.TypeParsers
{
    public sealed class SocketUserParser : TypeParser<SocketGuildUser>
    {
        public override ValueTask<TypeParserResult<SocketGuildUser>> ParseAsync(Parameter param, string value, CommandContext ctx,
            IServiceProvider provider)
        {
            var context = ctx as RiseContext;

            var users = context.Guild.Users;

            SocketGuildUser user = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[value.Length - 1] == '>' && 
                ulong.TryParse(value[2] == '!' ? value.Substring(3, value.Length - 4) : value.Substring(2, value.Length - 3), 
                    out var id)
                || ulong.TryParse(value, out id))
                user = users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                var hashIndex = value.LastIndexOf('#');
                if (hashIndex != -1 && hashIndex + 5 == value.Length)
                    user = users.FirstOrDefault(x =>
                        x.Username == value.Substring(0, value.Length - 5) &&
                        x.Discriminator == value.Substring(hashIndex + 1));
            }

            if (user != null)
                new TypeParserResult<SocketGuildUser>(user);

            IReadOnlyList<SocketGuildUser> matchingUsers = context.Guild != null
                ? users.Where(x => x.Username == value || x.Nickname == value).ToImmutableArray()
                : users.Where(x => x.Username == value).ToImmutableArray();

            if (matchingUsers.Count > 1)
                new TypeParserResult<SocketGuildUser>("Multiple matches found. Mention the user or use their ID.");

            if (matchingUsers.Count == 1)
                user = matchingUsers[0];

            return user == null
                ? new TypeParserResult<SocketGuildUser>("No user found matching the input.")
                : new TypeParserResult<SocketGuildUser>(user);
        }
    }
}
