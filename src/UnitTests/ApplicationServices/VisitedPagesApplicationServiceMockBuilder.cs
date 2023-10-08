using Moq;
using Scrap.Application.VisitedPages;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class VisitedPagesApplicationServiceMockBuilder
{
    private readonly Mock<IVisitedPageRepositoryFactory> _visitedPageRepositoryFactoryMock = new();

    public VisitedPagesApplicationServiceMockBuilder()
    {
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build()).Returns(VisitedPageRepositoryMock.Object);
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<IVisitedPageRepositoryOptions>()))
            .Returns(VisitedPageRepositoryMock.Object);
    }

    public Mock<IVisitedPageRepository> VisitedPageRepositoryMock { get; } = new();

    public IVisitedPagesApplicationService Build() =>
        new VisitedPagesApplicationService(_visitedPageRepositoryFactoryMock.Object);
}
