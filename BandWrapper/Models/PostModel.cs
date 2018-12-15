using Newtonsoft.Json;

namespace BandWrapper.Models
{
    internal class PostModel : BandBaseModel<PostDataModel>
    {
        [JsonProperty("result_code")]
        public override int ResultCode { get; set; }

        [JsonProperty("result_data")]
        public override PostDataModel ResultData { get; set; }
    }

    internal class PostDataModel
    {
        [JsonProperty("post")]
        public PostPostModel Post { get; set; }
    }

    internal class PostPostModel
    {
        [JsonProperty("schedule")]
        public PostScheduleModel Schedule { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    internal class PostScheduleModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_at")]
        public long StartAt { get; set; }

        [JsonProperty("end_at")]
        public long EndAt { get; set; }
    }
}
