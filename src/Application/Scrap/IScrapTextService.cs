﻿using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public interface IScrapTextService
{
    Task ScrapTextAsync(NewJobDto jobDto);
}