using Newtonsoft.Json;

namespace ClashWrapper.Models.War
{
    internal class WarMemberModel
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("townhallLevel")]
        public int TownhallLevel { get; set; }

        [JsonProperty("mapPosition")]
        public int MapPosition { get; set; }

        [JsonProperty("opponentAttacks")]
        public int OpponentAttacks { get; set; }

        [JsonProperty("bestOpponentAttack", NullValueHandling = NullValueHandling.Ignore)]
        public WarAttackModel BestOpponentAttack { get; set; }

        [JsonProperty("attacks", NullValueHandling = NullValueHandling.Ignore)]
        public WarAttackModel[] Attacks { get; set; }
    }
}
