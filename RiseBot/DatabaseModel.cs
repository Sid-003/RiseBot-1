using LiteDB;
using System.Collections.Generic;

namespace RiseBot
{
    public abstract class Entity
    {
        public abstract ulong Id { get; set; }
    }

    public class Guild : Entity
    {
        [BsonId(false)]
        public override ulong Id { get; set; }

        public string ClanTag { get; set; }
        
        public IList<GuildMember> GuildMembers { get; set; } = new List<GuildMember>();

        public ulong VerifiedRoleId { get; set; }
        public ulong WarChannelId { get; set; }
        public ulong StartTimeChannelId { get; set; }
        public ulong RepChannelId { get; set; }
    }

    public class GuildMember : Entity
    {
        public override ulong Id { get; set; }
        public string MainTag { get; set; }
        public IList<string> Tags { get; set; } = new List<string>();
    }
}
