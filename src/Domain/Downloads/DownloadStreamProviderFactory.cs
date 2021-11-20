using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        private class PollyMessageHandler: DelegatingHandler
        {
            private readonly IAsyncPolicy _policy;

            public PollyMessageHandler(IAsyncPolicy policy)
            {
                _policy = policy;
                InnerHandler = new HttpClientHandler();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _policy.ExecuteAsync(_ => base.SendAsync(request, cancellationToken), new Context(request.RequestUri.AbsoluteUri));
            }
        }
    }
}
