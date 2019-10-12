using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RiseBot
{
    public static class Extensions
    {
        public static IEnumerable<Type> FindTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static bool HasMentionPrefix(this IMessage message, IUser user, out string parsed)
        {
            var content = message.Content;
            parsed = "";
            if (content.Length <= 3 || content[0] != '<' || content[1] != '@')
                return false;

            var endPos = content.IndexOf('>');
            if (endPos == -1) return false;

            if (content.Length < endPos + 2 || content[endPos + 1] != ' ')
                return false;

            if (!MentionUtils.TryParseUser(content.Substring(0, endPos + 1), out var userId))
                return false;

            if (userId != user.Id) return false;
            parsed = content.Substring(endPos + 2);
            return true;
        }

        public static async Task ModifyAndWaitAsync(this IRole role, DiscordSocketClient client, Action<RoleProperties> action)
        {
            RoleProperties props = null;
            action.Invoke(props);

            var tcs = new TaskCompletionSource<bool>();

            Task RoleUpdatedAsync(SocketRole before, SocketRole after)
            {
                if(before.Id == role.Id)
                    tcs.SetResult(true);

                return Task.CompletedTask;
            }

            client.RoleUpdated += RoleUpdatedAsync;

            await role.ModifyAsync(action);

            await tcs.Task;

            client.RoleUpdated -= RoleUpdatedAsync;
        }
    }
}
