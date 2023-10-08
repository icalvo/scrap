namespace Scrap.Domain.Jobs;

public interface ILinkCalculatorOptions : IVisitedPageRepositoryOptions
{
    public bool FullScan { get; }
}