using Discord;
using Qmmands;
using RiseBot.Services;
using System.Threading.Tasks;

namespace RiseBot.Commands
{
    public abstract class RiseBase : ModuleBase<RiseContext>
    {
        public MessageService Message { get; set; }
        public DatabaseService Database { get; set; }
        public Guild Guild => Database.Guild;

        protected Task<IUserMessage> SendMessageAsync(string content, Embed embed = null)
            => Message.SendMessageAsync(Context, content, embed);
    }
}
