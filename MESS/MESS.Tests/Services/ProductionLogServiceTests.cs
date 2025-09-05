using MESS.Data.Context;
using MESS.Services.CRUD.ProductionLogs;
using MESS.Services.DTOs.ProductionLogs.Form;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using MESS.Data.Models;

namespace MESS.Tests.Services;

public class ProductionLogServiceTests : IDisposable
{
    private static readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");
    private readonly DbContextOptions<ApplicationContext> _options;

    static ProductionLogServiceTests()
    {
        _connection.Open();
    }

    public ProductionLogServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseSqlite(_connection)
            .Options;

        // Ensure clean database for each test
        using var context = new ApplicationContext(_options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
    }

    private ProductionLogService CreateService()
    {
        var dbFactory = new Mock<IDbContextFactory<ApplicationContext>>();
        dbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var ctx = new ApplicationContext(_options);
                ctx.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
                return ctx;
            });

        return new ProductionLogService(dbFactory.Object);
    }

    private static async Task<(int productId, int workInstructionId)> SeedProductAndWIAsync(string wiTitle, DbContextOptions<ApplicationContext> options)
    {
        await using var context = new ApplicationContext(options);
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=ON;");

        var product = new Product { Name = "Test Product" };
        var wi = new WorkInstruction { Title = wiTitle };

        context.Products.Add(product);
        context.WorkInstructions.Add(wi);
        await context.SaveChangesAsync();

        return (product.Id, wi.Id);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_EmptyLog_CreatesLogWithoutSerial()
    {
        var service = CreateService();
        var (productId, workInstructionId) = await SeedProductAndWIAsync("Test WI", _options);

        var formDtos = new List<ProductionLogFormDTO> { new() };
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-1", productId, workInstructionId);

        Assert.Equal(1, result.CreatedCount);
        Assert.Single(result.CreatedIds);

        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Null(log.ProductSerialNumber);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_WithWorkInstruction_AssignsCorrectWI()
    {
        var service = CreateService();
        var (productId, workInstructionId) = await SeedProductAndWIAsync("WI-100", _options);

        var formDtos = new List<ProductionLogFormDTO> { new() };
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-2", productId, workInstructionId);

        Assert.Equal(1, result.CreatedCount);
        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Equal(workInstructionId, log.WorkInstructionId);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_FinalAssemblyLog_SetsSerialNumber()
    {
        var service = CreateService();
        var (productId, workInstructionId) = await SeedProductAndWIAsync("Final Assembly WI", _options);

        var formDtos = new List<ProductionLogFormDTO> { new() { ProductSerialNumber = "SN-FINAL-001" } };
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-3", productId, workInstructionId);

        Assert.Equal(1, result.CreatedCount);
        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Equal("SN-FINAL-001", log.ProductSerialNumber);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_UpdatesExistingLog()
    {
        var service = CreateService();
        var (productId, workInstructionId) = await SeedProductAndWIAsync("Update Test WI", _options);

        var createForm = new ProductionLogFormDTO();
        var createResult = await service.SaveOrUpdateBatchAsync(new[] { createForm }, "tester", "op-4", productId, workInstructionId);

        var createdId = createResult.CreatedIds.First();
        var updateForm = new ProductionLogFormDTO { Id = createdId, ProductSerialNumber = "SN-UPDATED-001" };

        var updateResult = await service.SaveOrUpdateBatchAsync(new[] { updateForm }, "tester", "op-4", productId, workInstructionId);

        Assert.Equal(1, updateResult.UpdatedCount);
        var log = await service.GetByIdAsync(createdId);
        Assert.NotNull(log);
        Assert.Equal("SN-UPDATED-001", log.ProductSerialNumber);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_MultipleLogs_AllFinalAssemblyOrNone()
    {
        var service = CreateService();
        var (productId, workInstructionId) = await SeedProductAndWIAsync("Batch WI", _options);

        // Subassembly batch
        var subassemblyBatch = new List<ProductionLogFormDTO> { new(), new(), new() };
        var subassemblyResult = await service.SaveOrUpdateBatchAsync(subassemblyBatch, "tester", "op-1", productId, workInstructionId);
        Assert.Equal(3, subassemblyResult.CreatedCount);

        // Final assembly batch
        var finalAssemblyBatch = new List<ProductionLogFormDTO>
        {
            new() { ProductSerialNumber = "FA-1001" },
            new() { ProductSerialNumber = "FA-1002" },
            new() { ProductSerialNumber = "FA-1003" }
        };

        var finalAssemblyResult = await service.SaveOrUpdateBatchAsync(finalAssemblyBatch, "tester", "op-1", productId, workInstructionId);
        Assert.Equal(3, finalAssemblyResult.CreatedCount);

        // Verify all serials
        await using var context = new ApplicationContext(_options);
        var logsFinal = await context.ProductionLogs
            .Where(l => finalAssemblyResult.CreatedIds.Contains(l.Id))
            .ToListAsync();

        Assert.Equal(new[] { "FA-1001", "FA-1002", "FA-1003" },
            logsFinal.Select(l => l.ProductSerialNumber).ToArray());
    }

    public void Dispose()
    {
        // Do nothing; keep connection open for all tests
    }
}