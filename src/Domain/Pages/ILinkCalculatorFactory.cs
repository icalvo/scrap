using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public interface ILinkCalculatorFactory
{
    public ILinkCalculator Build(ILinkCalculatorOptions options);
}
