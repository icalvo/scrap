using Scrap.Domain.Downloads;

namespace Scrap.Infrastructure;

public class HttpClientDownloadStreamProvider : IDownloadStreamProvider
{
    private readonly HttpClient _httpClient;

    public HttpClientDownloadStreamProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Stream> GetStreamAsync(Uri url)
    {
        var response = await _httpClient.GetAsync(url);
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
