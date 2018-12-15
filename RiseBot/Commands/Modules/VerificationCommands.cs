using ClashWrapper;
using Discord.WebSocket;
using Qmmands;
using RiseBot.Commands.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseBot.Commands.Modules
{
    [RequireOwner(Group = "perms")]
    [RequireRole("fwa representative", "co-leaders")]
    public class VerificationCommands : RiseBase
    {
        public ClashClient Clash { get; set; }

        [Command("verify")]
        [RunMode(RunMode.Parallel)]
        public async Task VerifyMemberAsync(SocketGuildUser user, string userTag)
        {
            var members = await Clash.GetClanMembersAsync(Guild.ClanTag);

            var clanMember = members.FirstOrDefault(x =>
                string.Equals(x.Tag, userTag, StringComparison.InvariantCultureIgnoreCase));

            if (clanMember is null)
            {
                await SendMessageAsync("Tag not found in clan");
                return;
            }

            Guild.GuildMembers.Add(new GuildMember
            {
                Id = user.Id,
                MainTag = clanMember.Tag,
                Tags = new List<string>
                {
                    clanMember.Tag
                }
            });

            await user.ModifyAsync(x => x.Nickname = clanMember.Name);

            var verifiedRole = Context.Guild.GetRole(Guild.VerifiedRoleId);
            await user.AddRoleAsync(verifiedRole);

            var unverifiedRole = Context.Guild.GetRole(Guild.NotVerifiedRoleId);
            await user.RemoveRoleAsync(unverifiedRole);

            await SendMessageAsync("User has been verified");

            var verifiedChannel = Context.Guild.GetTextChannel(Guild.VerifiedChannelId);

            await verifiedChannel.SendMessageAsync($"{Context.User.Mention} verified {user.Mention}");

            var generalChannel = Context.Guild.GetTextChannel(Guild.GeneralId);

            await generalChannel.SendMessageAsync($"{user.Mention} welcome to Reddit Rise! Now that you are a verified member you have access to our channels!");
        }

        [Command("addtag")]
        [RunMode(RunMode.Parallel)]
        public async Task AddTagAsync(SocketGuildUser user, string userTag)
        {
            var members = await Clash.GetClanMembersAsync(Guild.ClanTag);

            var clanMember = members.FirstOrDefault(x =>
                string.Equals(x.Tag, userTag, StringComparison.InvariantCultureIgnoreCase));

            if (clanMember is null)
            {
                await SendMessageAsync("Tag not found in clan");
                return;
            }

            var guildMember = Guild.GuildMembers.FirstOrDefault(x => x.Id == user.Id);

            if (guildMember is null)
            {
                await SendMessageAsync("User is not verified");
                return;
            }

            guildMember.Tags.Add(clanMember.Tag);

            await SendMessageAsync("Tag has been added");
        }

        protected override Task AfterExecutedAsync(Command command)
        {
            return Database.WriteEntityAsync(Guild);
        }
    }
}
