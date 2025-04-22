using MESS.Data.Context;
using MESS.Services.ProductionLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;

namespace MESS.Tests.Services.ProductionLog;
using Data.Models;

public class ProductionLogServiceTests
{
    private readonly ProductionLogService _productionLogService;

    public ProductionLogServiceTests()
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
        dbFactory.Setup(f => 
                f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationContext(options));

        _productionLogService = new ProductionLogService(dbFactory.Object);
    }

    [Fact]
    public async Task CreateLog_EmptyLog_SuccessfullyAppliesDefaultValues()
    {
        var productionLogToAdd = new ProductionLog();

        var productionLogToAddId = await _productionLogService.CreateAsync(productionLogToAdd);
        
        Assert.Equal(1,productionLogToAddId);
    }
}