using System;
using System.Diagnostics;
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
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private readonly Ratelimiter _ratelimiter;

        private const int MaxRequests = 5;
        private const long RequestTime = 5000;

        private const string BaseUrl = "https://api.clashofclans.com";
        
        public RequestClient(ClashClient client, ClashClientConfig config)
        {
            _client = client;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Token}");

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

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
            {
                Content = new StringContent(parameters.BuildContent())
            };

            var sw = new Stopwatch();
            sw.Start();

            try
            {
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    _semaphore.Release();
                    sw.Stop();

                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    //await _client.InternalLogReceivedAsync($"GET {endpoint} {sw.ElapsedMilliseconds}ms");

                    if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<T>(content);

                    var model = JsonConvert.DeserializeObject<ErrorModel>(content);
                    var error = new ErrorMessage(model);

                    await _client.InternalErrorReceivedAsync(error);
                    return default;
                }
            }
            catch (Exception ex)
            {
                await _client.InternalLogReceivedAsync(ex.ToString());
                _semaphore.Release();
                return default;
            }
        }
    }
}
