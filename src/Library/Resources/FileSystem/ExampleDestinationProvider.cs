using System;
using System.Threading.Tasks;
using Scrap.Pages;
using Scrap.Resources.FileSystem.Extensions;

namespace Scrap.Resources.FileSystem
{
    public class ExampleDestinationProvider: IDestinationProvider
    {
        public async Task<string> GetDestinationAsync(
            Uri resourceUrl,
            string destinationRootFolder,
            Page page)
        {
            return
                destinationRootFolder
                    .C((await page.LinkedDoc("//*[contains(@class='back-to-gallery')]//a]"))?.Text("//a[contains(@href='/gallery/artist']"))
                    .C(page.Text("//h1") ?? "")
                    .C(page.Uri.CleanSegments()[^1] + resourceUrl.Extension())
                    .ToPath();
        }
    }
}