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

        public char Prefix { get; set; }

        public string ClanTag { get; set; }
        
        public IList<GuildMember> GuildMembers { get; set; } = new List<GuildMember>();
        public IList<FWARep> FWAReps { get; set; } = new List<FWARep>();

        public ulong WelcomeChannelId { get; set; }
        public ulong VerifiedRoleId { get; set; }
        public ulong VerifiedChannelId { get; set; }
        public ulong NotVerifiedRoleId { get; set; }
        public ulong WarChannelId { get; set; }
        public ulong StartTimeChannelId { get; set; }
        public ulong RepChannelId { get; set; }
        public ulong GeneralId { get; set; }
    }

    public class GuildMember : Entity
    {
        public override ulong Id { get; set; }
        public string MainTag { get; set; }
        public IList<string> Tags { get; set; } = new List<string>();
    }

    public class FWARep : Entity
    {
        public override ulong Id { get; set; }
        public double TimeZone { get; set; }
    }
}
