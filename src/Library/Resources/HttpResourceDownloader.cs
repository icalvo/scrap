using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace Scrap.Resources
{
    public class HttpResourceDownloader : IResourceDownloader
    {
        private readonly ILogger<HttpResourceDownloader> _logger;
        private readonly IAsyncPolicy _policy;
        private static readonly HttpClient HttpClient = new();

        public HttpResourceDownloader(ILogger<HttpResourceDownloader> logger, IAsyncPolicy policy)
        {
            _logger = logger;
            _policy = policy;
        }

        public async Task DownloadFileAsync(
            Uri uri,
            Stream outputStream)
        {
            await _policy.ExecuteAsync(async _ =>
            {
                _logger.LogInformation("GET {Uri}", uri);
                using HttpResponseMessage response = await HttpClient.GetAsync(uri);
                await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                await streamToReadFrom.CopyToAsync(outputStream);
            },
                new Context(uri.AbsoluteUri));
        }
    }
}