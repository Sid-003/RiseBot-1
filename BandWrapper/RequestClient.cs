using BandWrapper.Entities;
using BandWrapper.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BandWrapper
{
    internal class RequestClient
    {
        private readonly BandClient _client;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;

        private const string BaseUrl = "https://openapi.band.us";

        public RequestClient(BandClient client, BandClientConfig config)
        {
            _client = client;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Token}");

            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<T> SendAsync<T>(string endpoint)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var sw = new Stopwatch();
            sw.Start();

            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                _semaphore.Release();
                sw.Stop();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                await _client.InternalLogReceivedAsync($"GET {endpoint} {sw.ElapsedMilliseconds}ms");

                var model = JsonConvert.DeserializeObject<ErrorMessageModel>(content);

                if(model.ResultCode == 1) return JsonConvert.DeserializeObject<T>(content);
                
                var error = new ErrorMessage(model);

                await _client.InternalErrorReceivedAsync(error).ConfigureAwait(false);
                return default;
            }
        }
    }
}
