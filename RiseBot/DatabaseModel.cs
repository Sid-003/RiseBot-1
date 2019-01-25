using LiteDB;
using System.Collections.Generic;
using RiseBot.Entities;

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
        public IList<Event> Events { get; set; } = new List<Event>();

        public ulong WelcomeChannelId { get; set; }
        public ulong VerifiedRoleId { get; set; }
        public ulong VerifiedChannelId { get; set; }
        public ulong NotVerifiedRoleId { get; set; }
        public ulong WarChannelId { get; set; }
        public ulong StartTimeChannelId { get; set; }
        public ulong RepChannelId { get; set; }
        public ulong GeneralId { get; set; }
        public ulong InWarRoleId { get; set; }
        public ulong EventRoleId { get; set; }
        public ulong EventChannelId { get; set; }

        public ulong EventMessageId { get; set; }
    }

    public class GuildMember : Entity
    {
        public override ulong Id { get; set; }
        public string MainTag { get; set; }
        public int TotalWars { get; set; }
        public int MissedAttacks { get; set; }
        public IList<string> Tags { get; set; } = new List<string>();
    }

    public class FWARep : Entity
    {
        public override ulong Id { get; set; }
        public double TimeZone { get; set; }
    }

    public class Event : Entity, IRemovable
    {
        public override ulong Id { get; set; }

        public string Description { get; set; }

        public bool Mention { get; set; }

        public int Month { get; set; }
        public int Day { get; set; }
        public int End { get; set; }

        public long WhenToRemove { get; set; }
    }
}
