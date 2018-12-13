using System.Net.Http;

namespace ClashWrapper
{
    public class ClashClientConfig
    {
        public string Token { get; set; }
        public HttpClient HttpClient { get; set; }
    }
}
