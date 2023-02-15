using Moq;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class VisitedPagesApplicationServiceMockBuilder
{
    private readonly Mock<IOptionalParameterFactory<Job, IPageMarkerRepository>> _pageMarkerRepositoryFactoryMock =
        new();

    public VisitedPagesApplicationServiceMockBuilder()
    {
        _pageMarkerRepositoryFactoryMock.SetupFactory(PageMarkerRepositoryMock.Object);
    }

    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();

    public IVisitedPagesApplicationService Build() =>
        new VisitedPagesApplicationService(_pageMarkerRepositoryFactoryMock.Object);
}
