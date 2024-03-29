﻿using Discord.WebSocket;
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

        [Command("setwarrole")]
        public Task SetWarRoleAsync(SocketRole role)
        {
            Guild.InWarRoleId = role.Id;
            return SendMessageAsync("War role has been set");
        }

        [Command("seteventrole")]
        public Task SetEventRoleAsync(SocketRole role)
        {
            Guild.EventRoleId = role.Id;
            return SendMessageAsync("Event role has been set");
        }

        [Command("seteventchannel")]
        public Task SetEventChannelAsync(SocketTextChannel channel)
        {
            Guild.EventChannelId = channel.Id;
            return SendMessageAsync("Event channel has been set");
        }

        [Command("setcorole")]
        public Task SetCoRoleAsync(SocketRole role)
        {
            Guild.CoRoleId = role.Id;
            return SendMessageAsync("Co role has been set");
        }

        [Command("setelderrole")]
        public Task SetElderRoleAsync(SocketRole role)
        {
            Guild.ElderRoleId = role.Id;
            return SendMessageAsync("Elder role has been set");
        }

        [Command("setnonotifsrole")]
        public Task SetNoNotifsRoleAsync(SocketRole role)
        {
            Guild.NoNotifsRoleId = role.Id;
            return SendMessageAsync("No notifs role has been set");
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
                $"General: {Guild.GeneralId}",
                $"WarRole: {Guild.InWarRoleId}",
                $"EventRole: {Guild.EventRoleId}",
                $"EventChannel: {Guild.EventChannelId}",
                $"CoRole: {Guild.CoRoleId}",
                $"ElderTole: {Guild.ElderRoleId}",
                $"No notifs role: {Guild.NoNotifsRoleId}"
            };

            return SendMessageAsync(string.Join('\n', message));
        }

        protected override ValueTask AfterExecutedAsync()
        {
            Database.UpdateGuild();
            return default;
        }
    }
}
