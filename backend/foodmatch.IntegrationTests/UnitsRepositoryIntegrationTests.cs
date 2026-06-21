using foodmatch.Units.Model;
using foodmatch.Units.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace foodmatch.IntegrationTests;

public class UnitsRepositoryIntegrationTests : DatabaseIntegrationTest
{
    private UnitRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var logger = ServiceProvider.GetRequiredService<ILogger<UnitRepository>>();
        _repository = new UnitRepository(DbContext, logger);
    }

    [Fact]
    public async Task AddUnit_ShouldAddUnitToDatabase()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UnitRepository>>();
        var repository = new UnitRepository(DbContext, mockLogger.Object);
        var unit = new Unit { UnitId = 1, Name = "Kilogram" };

        // Act
        await repository.AddUnitAsync(unit);
        var result = await repository.GetUnitByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Kilogram", result.Name);
    }
}

