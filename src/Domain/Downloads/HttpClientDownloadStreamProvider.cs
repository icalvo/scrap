using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scrap.Downloads
{
    public class HttpClientDownloadStreamProvider : IDownloadStreamProvider
    {
        private readonly HttpClient _httpClient;

        public HttpClientDownloadStreamProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Stream> GetStreamAsync(Uri url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }

            throw new HttpRequestException(
                $"Could not retrieve {url}. Status code {response.StatusCode}",
                null,
                response.StatusCode);
        }
    }
}
