using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Scrap.Application;
using Scrap.Common;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class VisitedPagesApplicationServiceMockBuilder
{
    private readonly Mock<IPageMarkerRepositoryFactory> _pageMarkerRepositoryFactoryMock = new();

    public VisitedPagesApplicationServiceMockBuilder()
    {
        _pageMarkerRepositoryFactoryMock.Setup(x => x.Build()).Returns(PageMarkerRepositoryMock.Object);
        _pageMarkerRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageMarkerRepositoryMock.Object);
    }

    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();

    public IVisitedPagesApplicationService Build() =>
        new VisitedPagesApplicationService(_pageMarkerRepositoryFactoryMock.Object);
}
