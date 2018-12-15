using Newtonsoft.Json;

namespace ClashWrapper.Models.ClanMembers
{
    internal class LeagueModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconUrls")]
        public IconUrls IconUrls { get; set; }
    }
}
