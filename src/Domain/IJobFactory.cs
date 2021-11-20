using Scrap.Jobs;

namespace Scrap
{
    public interface IJobFactory
    {
        Job Create(NewJobDto newJobDto);
    }
}