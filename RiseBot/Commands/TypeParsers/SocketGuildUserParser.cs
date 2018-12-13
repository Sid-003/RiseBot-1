using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace RiseBot.Commands.TypeParsers
{
    public class SocketGuildUserParser : TypeParser<SocketGuildUser>
    {
        public override Task<TypeParserResult<SocketGuildUser>> ParseAsync(string input, ICommandContext originContext, IServiceProvider _)
        {
            if(!(originContext is RiseContext context))
                return Task.FromResult(new TypeParserResult<SocketGuildUser>("Invalid context type"));
            
            var users = context.Guild.Users;

            if (MentionUtils.TryParseUser(input, out var id))
            {
                return Task.FromResult(new TypeParserResult<SocketGuildUser>(context.Guild.GetUser(id)));
            }

            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                return Task.FromResult(new TypeParserResult<SocketGuildUser>(context.Guild.GetUser(id)));
            }

            var index = input.LastIndexOf('#');

            if (index >= 0)
            {
                var username = input.Substring(0, index);

                if (ushort.TryParse(input.Substring(index + 1), out var discim))
                {
                    var user = users.FirstOrDefault(x =>
                        x.DiscriminatorValue == discim &&
                        string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));

                    return Task.FromResult(new TypeParserResult<SocketGuildUser>(user));
                }
            }

            var foundUser = users.FirstOrDefault(x =>
                string.Equals(input, x.Username, StringComparison.OrdinalIgnoreCase));

            if(!(foundUser is null))
                return Task.FromResult(new TypeParserResult<SocketGuildUser>(foundUser));

            foundUser = users.FirstOrDefault(x =>
                string.Equals(input, x.Nickname, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(!(foundUser is null) ? new TypeParserResult<SocketGuildUser>(foundUser) : new TypeParserResult<SocketGuildUser>("Failed to find user"));
        }
    }
}
