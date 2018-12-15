using Newtonsoft.Json;

namespace ClashWrapper.Models.ClanMembers
{
    internal class ClanMemberModel
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("expLevel")]
        public int Level { get; set; }

        [JsonProperty("league")]
        public LeagueModel League { get; set; }

        [JsonProperty("trophies")]
        public int Trophies { get; set; }

        [JsonProperty("versusTrophies")]
        public int VersusTrophies { get; set; }

        [JsonProperty("clanRank")]
        public int ClanRank { get; set; }

        [JsonProperty("previousClanRank")]
        public int PreviousClanRank { get; set; }

        [JsonProperty("donations")]
        public int Donations { get; set; }

        [JsonProperty("donationsReceived")]
        public int DonationsReceived { get; set; }
    }
}
