using System;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Scrap.Pages;

namespace Scrap.Resources
{
    public interface IResourceRepository
    {
        Task UpsertResourceAsync(
            Uri resourceUrl,
            Page page);
    }
}