using Newtonsoft.Json;

namespace ClashWrapper.Models.War
{
    internal class WarClanModel
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("badgeUrls")]
        public BadgeUrlModel BadgeUrls { get; set; }

        [JsonProperty("clanLevel")]
        public int ClanLevel { get; set; }

        [JsonProperty("attacks")]
        public int Attacks { get; set; }

        [JsonProperty("stars")]
        public int Stars { get; set; }

        [JsonProperty("destructionPercentage")]
        public double DestructionPercentage { get; set; }

        [JsonProperty("members")]
        public WarMemberModel[] Members { get; set; }
    }
}
