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

        public async Task<Stream> GetStreamAsync(Uri resourceUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(resourceUrl);
            return await response.Content.ReadAsStreamAsync();
        }
    }
}