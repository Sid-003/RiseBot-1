using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.TypeParsers
{
    public sealed class SocketRoleParser : TypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter param, string value, CommandContext ctx, IServiceProvider provider)
        {
            var context = ctx as RiseContext;
            if (context.Guild == null)
                return new TypeParserResult<SocketRole>("This command must be used in a guild.");

            SocketRole role = null;
            if (value.Length > 3 && value[0] == '<' && value[1] == '@' && value[2] == '&' && value[value.Length - 1] == '>' && ulong.TryParse(value.Substring(3, value.Length - 4), out var id)
                || ulong.TryParse(value, out id))
                role = context.Guild.Roles.FirstOrDefault(x => x.Id == id);

            if (role == null)
                role = context.Guild.Roles.FirstOrDefault(x => x.Name == value);

            return role == null
                ? new TypeParserResult<SocketRole>("No role found matching the input.")
                : new TypeParserResult<SocketRole>(role);
        }
    }
}
