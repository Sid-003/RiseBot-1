using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ClashWrapper.Entities;
using ClashWrapper.Models;
using ClashWrapper.RequestParameters;
using Newtonsoft.Json;

namespace ClashWrapper
{
    internal class RequestClient
    {
        private readonly ClashClient _client;
        private readonly ClashClientConfig _config;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private readonly Ratelimiter _ratelimiter;

        private const int MaxRequests = 5;
        private const long RequestTime = 5000;

        private const string BaseUrl = "https://api.clashofclans.com/v1";
        
        public RequestClient(ClashClient client, ClashClientConfig config)
        {
            _client = client;
            _config = config;
            _httpClient = config.HttpClient ?? new HttpClient();
            _semaphore = new SemaphoreSlim(1);
            _ratelimiter = new Ratelimiter(MaxRequests, RequestTime);
        }

        public async Task<T> SendAsync<T>(string endpoint, BaseParameters parameters = null)
        {
            if(endpoint[0] != '/')
                throw new ArgumentException($"{nameof(endpoint)} must start with a '/'");

            await _semaphore.WaitAsync().ConfigureAwait(false);
            await _ratelimiter.WaitAsync().ConfigureAwait(false);

            parameters = parameters ?? new EmptyParameters();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.Token}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{endpoint}")
            {
                Content = new StringContent(parameters.BuildContent())
            };

            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                _semaphore.Release();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<T>(content);

                var model = JsonConvert.DeserializeObject<ErrorModel>(content);
                var error = new ErrorMessage(model);

                await _client.InternalErrorReceivedAsync(error);
                return default;

            }
        }
    }
}
