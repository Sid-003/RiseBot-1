using Newtonsoft.Json;

namespace ClashWrapper.Models.War
{
    internal class WarAttackModel
    {
        [JsonProperty("attackerTag")]
        public string AttackerTag { get; set; }

        [JsonProperty("defenderTag")]
        public string DefenderTag { get; set; }

        [JsonProperty("stars")]
        public int Stars { get; set; }

        [JsonProperty("destructionPercentage")]
        public int DestructionPercentage { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }
}
