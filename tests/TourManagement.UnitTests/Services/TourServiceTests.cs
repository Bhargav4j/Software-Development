using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Application.Services;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using Xunit;

namespace TourManagement.UnitTests.Services;

public class TourServiceTests
{
    private readonly Mock<ITourRepository> _mockRepository;
    private readonly Mock<ILogger<TourService>> _mockLogger;
    private readonly TourService _service;

    public TourServiceTests()
    {
        _mockRepository = new Mock<ITourRepository>();
        _mockLogger = new Mock<ILogger<TourService>>();
        _service = new TourService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllToursAsync_ShouldReturnAllTours()
    {
        var tours = new List<Tour>
        {
            new Tour { Id = 1, TourName = "Tour 1", Place = "Place 1", Days = 5, Price = 1000 },
            new Tour { Id = 2, TourName = "Tour 2", Place = "Place 2", Days = 7, Price = 1500 }
        };

        _mockRepository.Setup(r => r.GetAllAsync(default)).ReturnsAsync(tours);

        var result = await _service.GetAllToursAsync();

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(tours);
    }

    [Fact]
    public async Task CreateTourAsync_ShouldCreateTour()
    {
        var tour = new Tour
        {
            TourName = "New Tour",
            Place = "New Place",
            Days = 5,
            Price = 1000
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Tour>(), default))
            .ReturnsAsync((Tour t, CancellationToken ct) => { t.Id = 1; return t; });

        var result = await _service.CreateTourAsync(tour);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }
}
