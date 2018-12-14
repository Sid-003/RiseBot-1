using Newtonsoft.Json;

namespace BandWrapper.Models
{
    internal class ErrorMessageModel : BandBaseModel<ErrorDataModel>
    {
        [JsonProperty("result_code")]
        public override int ResultCode { get; set; }

        [JsonProperty("result_data")]
        public override ErrorDataModel ResultData { get; set; }
    }

    internal class ErrorDataModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("detail")]
        public ErrorDetailModel Detail { get; set; }
    }

    internal class ErrorDetailModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Descroption { get; set; }
    }
}
