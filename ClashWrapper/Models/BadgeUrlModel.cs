using Newtonsoft.Json;

namespace ClashWrapper.Models
{
    internal class BadgeUrlModel
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }
    }
}
