using System;
using System.Net.Http;
using Polly;

namespace Scrap.Downloads
{
    public class DownloadStreamProviderFactory
    {
        public IDownloadStreamProvider Build(string protocol, IAsyncPolicy policy)
        {
            switch (protocol)
            {
                case "http":
                case "https":
                    var httpClient = new HttpClient(new PollyMessageHandler(policy));
                    return new HttpClientDownloadStreamProvider(httpClient);
                default:
                    throw new ArgumentException($"Unknown URI protocol {protocol}", nameof(protocol));
            }
        }
    }
}