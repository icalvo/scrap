using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages
{
    public class HttpHelper
    {
        private readonly ILogger<HttpHelper> _logger;
        private static readonly HttpClient HttpClient = new();

        public HttpHelper(ILogger<HttpHelper> logger)
        {
            _logger = logger;
        }

        public async Task DownloadFileAsync(
            Uri uri,
            string outputPath)
        {
            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    Thread.Sleep(1000);
                    using HttpResponseMessage response = await HttpClient.GetAsync(uri);
                    // using HttpResponseMessage response = await Trol(uri);
                    await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                    await using Stream streamToWriteTo = File.Open(outputPath, FileMode.Create);
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error: " + ex.Message);
                }
            }

            throw new InvalidOperationException("Retried download too many times");
        }
    }
}