using System;
using System.Threading.Tasks;

namespace Scrap.Pages;

public interface IPageMarkerRepository
{
    Task<bool> ExistsAsync(Uri uri);
    Task UpsertAsync(Uri link);
}