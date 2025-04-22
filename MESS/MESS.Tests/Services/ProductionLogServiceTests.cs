using MESS.Data.Context;
using MESS.Services.ProductionLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;

namespace MESS.Tests.Services.ProductionLog;
using Data.Models;

public class ProductionLogServiceTests
{
    private static ProductionLogService MockProductionLogService ()
    {
        // Configure DbContextOptions to use SQLite with a file-based database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseSqlite("Data Source=TestDatabase.db")
            .Options;

        // Ensure the database is created before tests run
        using (var context = new ApplicationContext(options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        // Set up the DbContextFactory to return a properly configured ApplicationContext instance
        var dbFactory = new Mock<IDbContextFactory<ApplicationContext>>();
        dbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationContext(options)); // Create new instance on each call

        
        return new ProductionLogService(dbFactory.Object);
    }

    [Fact]
    public async Task CreateLog_EmptyLog_SuccessfullyCreatesLog()
    {
        var productionLogToAdd = new ProductionLog();

        var productionLogToAddId = await MockProductionLogService().CreateAsync(productionLogToAdd);
        
        Assert.Equal(1,productionLogToAddId);
    }
    
    [Fact]
    public async Task CreateLog_SetsTimestamps()
    {
        // Arrange
        var service = MockProductionLogService();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var productionLog = new ProductionLog();

        // Act
        var createdId = await service.CreateAsync(productionLog);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        // Assert
        var savedLog = await service.GetByIdAsync(createdId);
        Assert.NotNull(savedLog);
        Assert.True(savedLog.CreatedOn >= before && savedLog.CreatedOn <= after);
        Assert.True(savedLog.LastModifiedOn >= before && savedLog.LastModifiedOn <= after);
    }

    [Fact]
    public async Task CreateLog_DbContextException_ReturnsNegativeOne()
    {
        // Mock the factory to throw exception
        var dbFactory = new Mock<IDbContextFactory<ApplicationContext>>();
        dbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));
    
        var service = new ProductionLogService(dbFactory.Object);
        var productionLog = new ProductionLog();

        // Act
        var result = await service.CreateAsync(productionLog);

        // Assert
        Assert.Equal(-1, result);
    }
}