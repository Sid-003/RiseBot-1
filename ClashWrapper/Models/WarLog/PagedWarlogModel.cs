using Newtonsoft.Json;

namespace ClashWrapper.Models.WarLog
{
    internal class PagedWarlogModel
    {
        [JsonProperty("items")]
        public WarLogModel[] WarLogs { get; set; }

        [JsonProperty("paging")]
        public PagingModel Paging { get; set; }
    }
}
