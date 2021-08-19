using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scrap.CommandLine
{
    public static class HttpHelper
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task DownloadFileAsync(
            Uri uri,
            string outputPath)
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    using HttpResponseMessage response = await HttpClient.GetAsync(uri);
                    await using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();
                    await using Stream streamToWriteTo = File.Open(outputPath, FileMode.Create);
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}