using Newtonsoft.Json;

namespace ClashWrapper.Models
{
    internal class ErrorModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
