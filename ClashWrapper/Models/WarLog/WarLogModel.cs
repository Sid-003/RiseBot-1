using Newtonsoft.Json;

namespace ClashWrapper.Models.WarLog
{
    internal class WarLogModel
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("teamSize")]
        public int TeamSize { get; set; }

        [JsonProperty("clan")]
        public WarLogClanModel Clan { get; set; }

        [JsonProperty("opponent")]
        public WarLogClanModel Opponent { get; set; }
    }
}
