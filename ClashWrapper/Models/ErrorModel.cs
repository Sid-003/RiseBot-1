using Newtonsoft.Json;

namespace ClashWrapper.Models
{
    internal class ErrorModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
