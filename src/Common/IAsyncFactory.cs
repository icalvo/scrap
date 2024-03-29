﻿namespace Scrap.Common;

public interface IAsyncFactory<in TIn, TOut>
{
    public Task<TOut> BuildAsync(TIn param);
}
