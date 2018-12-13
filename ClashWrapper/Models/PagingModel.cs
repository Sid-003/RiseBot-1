using Newtonsoft.Json;

namespace ClashWrapper.Models
{
    internal class PagingModel
    {
        [JsonProperty("cursors")]
        public Cursors Cursors { get; set; }
    }

    internal class Cursors
    {
        [JsonProperty("after", NullValueHandling = NullValueHandling.Ignore)]
        public string After { get; set; }

        [JsonProperty("before", NullValueHandling = NullValueHandling.Ignore)]
        public string Before { get; set; }
    }
}
