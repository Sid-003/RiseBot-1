using Newtonsoft.Json;

namespace ClashWrapper.Models.War
{
    internal class CurrentWarModel
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("teamSize")]
        public int TeamSize { get; set; }

        [JsonProperty("preparationStartTime")]
        public string PreparationStartTime { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("clan")]
        public WarClanModel Clan { get; set; }

        [JsonProperty("opponent")]
        public WarClanModel Opponent { get; set; }
    }
}
