using System;
using Scrap.Pages;
using Scrap.Resources.FileSystem.Extensions;

namespace Scrap.Resources.FileSystem
{
    public class InternalDestinationProvider2: IDestinationProvider
    {
        public string GetDestination(
            Uri resourceUrl,
            string destinationRootFolder,
            Page page)
        {
            return
                destinationRootFolder
                    .C(page.LinkedDoc("//*[contains(@class='back-to-gallery')]//a]")?.Text("//a[contains(@href='/gallery/artist']"))
                    .C(page.Text("//h1") ?? "")
                    .C(page.Uri.CleanSegments()[^1] + resourceUrl.Extension()).ToPath();
        }
    }
}