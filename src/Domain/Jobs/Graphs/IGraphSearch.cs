using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scrap.Jobs.Graphs;

public interface IGraphSearch
{
    IAsyncEnumerable<T> SearchAsync<TRef, T>(
        TRef startRef,
        Func<TRef, Task<T>> visit,
        Func<T, IAsyncEnumerable<TRef>> adjacentRefs);
}