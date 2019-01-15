using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands.Checks;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    [RequireOwner(Group = "perms")]
    [RequireRole("secretary", "fwa representatives", "co-leaders", Group = "perms")]
    public class SettingsCommands : RiseBase
    {
        [Command("setprefix")]
        public Task SetPrefixAsync(char prefix)
        {
            Guild.Prefix = prefix;
            return SendMessageAsync("Prefix has been successfully set");
        }

        [Command("setclantag")]
        public Task SetClantagAsync(string clantag)
        {
            Guild.ClanTag = clantag;
            return SendMessageAsync("Clan tag has been successfully set");
        }

        [Command("setwelcomechannel")]
        public Task SetWelcomeChannelAsync(SocketTextChannel channel)
        {
            Guild.WelcomeChannelId = channel.Id;
            return SendMessageAsync("Welcome channel has been set");
        }

        [Command("setverifiedrole")]
        public Task SetVerifiedAsync(SocketRole role)
        {
            Guild.VerifiedRoleId = role.Id;
            return SendMessageAsync("Verified role has been set");
        }

        [Command("setunverifiedrole")]
        public Task SetUnverifiedAsync(SocketRole role)
        {
            Guild.NotVerifiedRoleId = role.Id;
            return SendMessageAsync("Not verified role has been set");
        }

        [Command("setverifiedchannel")]
        public Task SetVerifiedChannelAsync(SocketTextChannel channel)
        {
            Guild.VerifiedChannelId = channel.Id;
            return SendMessageAsync("Verified channel has been set");
        }

        [Command("setwarchannel")]
        public Task SetWarChannelAsync(SocketTextChannel channel)
        {
            Guild.WarChannelId = channel.Id;
            return SendMessageAsync("War channel has been set");
        }

        [Command("setstartchannel")]
        public Task SetStartChannelAsync(SocketTextChannel channel)
        {
            Guild.StartTimeChannelId = channel.Id;
            return SendMessageAsync("Start time channel has been set");
        }

        [Command("setrepchannel")]
        public Task SetRepChannelAsync(SocketTextChannel channel)
        {
            Guild.RepChannelId = channel.Id;
            return SendMessageAsync("Rep channel has been set");
        }

        [Command("setgeneralchannel")]
        public Task SetGeneralChannelAsync(SocketTextChannel channel)
        {
            Guild.GeneralId = channel.Id;
            return SendMessageAsync("General channel has been set");
        }

        [Command("viewsettings")]
        public Task ViewSettingsAsync()
        {
            var message = new[] 
            {
                $"Prefix: {Guild.Prefix}",
                $"Clantag: {Guild.ClanTag}",
                $"Welcome: {Guild.WelcomeChannelId}",
                $"VerifiedRole: {Guild.VerifiedRoleId}",
                $"VerifiedChannel: {Guild.WelcomeChannelId}",
                $"NotVerified: {Guild.NotVerifiedRoleId}",
                $"War: {Guild.WarChannelId}",
                $"Start: {Guild.StartTimeChannelId}",
                $"Rep: {Guild.RepChannelId}",
                $"General: {Guild.GeneralId}"
            };

            return SendMessageAsync(string.Join('\n', message));
        }

        protected override Task AfterExecutedAsync(Command command)
        {
            return Database.WriteEntityAsync(Guild);
        }
    }
}
