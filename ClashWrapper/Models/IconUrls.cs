using Newtonsoft.Json;

namespace ClashWrapper.Models
{
    internal class IconUrls
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("tiny")]
        public string Tiny { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }
    }
}
