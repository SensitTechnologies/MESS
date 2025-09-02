using MESS.Data.Context;
using MESS.Services.CRUD.ProductionLogs;
using MESS.Services.DTOs.ProductionLogs.Form;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MESS.Tests.Services.ProductionLog;
using Data.Models;

public class ProductionLogServiceTests
{
    private const string CONNECTION_STRING = "Data Source=ProductionLogServiceTestDatabase.db";

    /// <summary>
    /// Creates a new ProductionLogService backed by an SQLite database.
    /// The database is reset (deleted/recreated) before each test to ensure isolation.
    /// </summary>
    private static ProductionLogService MockProductionLogService()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseSqlite(CONNECTION_STRING)
            .Options;

        // Reset DB for each test
        using (var context = new ApplicationContext(options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        // Mock the factory so every call gives a fresh ApplicationContext
        var dbFactory = new Mock<IDbContextFactory<ApplicationContext>>();
        dbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationContext(options));

        return new ProductionLogService(dbFactory.Object);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_EmptyLog_CreatesLogWithoutSerial()
    {
        // Arrange: no product serial → treated as a subassembly
        var service = MockProductionLogService();
        var formDtos = new List<ProductionLogFormDTO> { new() };

        // Act
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-1", 1, 1);

        // Assert: log should be created, but with no serial number
        Assert.Equal(1, result.CreatedCount);
        Assert.Single(result.CreatedIds);

        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Null(log.ProductSerialNumber); // confirms subassembly behavior
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_WithWorkInstruction_AssignsCorrectWI()
    {
        var service = MockProductionLogService();

        // Arrange: create and persist a work instruction
        var wi = new WorkInstruction { Title = "WI-100" };
        using (var context = new ApplicationContext(
                   new DbContextOptionsBuilder<ApplicationContext>()
                       .UseSqlite(CONNECTION_STRING)
                       .Options))
        {
            context.WorkInstructions.Add(wi);
            await context.SaveChangesAsync();
        }

        var formDtos = new List<ProductionLogFormDTO> { new() };

        // Act: save a log referencing the work instruction
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-2", 1, wi.Id);

        // Assert: log should reference the correct WI
        Assert.Equal(1, result.CreatedCount);

        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Equal(wi.Id, log.WorkInstructionId);
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_FinalAssemblyLog_SetsSerialNumber()
    {
        var service = MockProductionLogService();

        // Arrange: final assembly form → has a serial number
        var formDtos = new List<ProductionLogFormDTO>
        {
            new() { ProductSerialNumber = "SN-FINAL-001" }
        };

        // Act
        var result = await service.SaveOrUpdateBatchAsync(formDtos, "tester", "op-3", 1, 2);

        // Assert: log should be created with the serial number preserved
        Assert.Equal(1, result.CreatedCount);

        var log = await service.GetByIdAsync(result.CreatedIds.First());
        Assert.NotNull(log);
        Assert.Equal("SN-FINAL-001", log.ProductSerialNumber); // final assembly rule
    }

    [Fact]
    public async Task SaveOrUpdateBatchAsync_UpdatesExistingLog()
    {
        var service = MockProductionLogService();

        // Arrange: first create a log without serial
        var createForm = new ProductionLogFormDTO();
        var createResult = await service.SaveOrUpdateBatchAsync(
            [createForm], "tester", "op-4", 1, 3);

        var createdId = createResult.CreatedIds.First();

        // Act: update the same log, this time adding a serial
        var updateForm = new ProductionLogFormDTO
        {
            Id = createdId,
            ProductSerialNumber = "SN-UPDATED-001"
        };

        var updateResult = await service.SaveOrUpdateBatchAsync(
            [updateForm], "tester", "op-4", 1, 3);

        // Assert: should update existing record, not create new
        Assert.Equal(1, updateResult.UpdatedCount);

        var log = await service.GetByIdAsync(createdId);
        Assert.NotNull(log);
        Assert.Equal("SN-UPDATED-001", log.ProductSerialNumber);
    }
    
    [Fact]
    public async Task SaveOrUpdateBatchAsync_MultipleLogs_AllFinalAssemblyOrNone()
    {
        var service = MockProductionLogService();
        var createdBy = "tester";
        var operatorId = "op1";

        // Arrange: create a single WI (all logs in batch share this)
        int workInstructionId;
        await using (var context = new ApplicationContext(
                       new DbContextOptionsBuilder<ApplicationContext>()
                           .UseSqlite(CONNECTION_STRING)
                           .Options))
        {
            var wi = new WorkInstruction { Title = "Batch WI" };
            context.WorkInstructions.Add(wi);
            await context.SaveChangesAsync();
            workInstructionId = wi.Id;
        }

        // --- SUBASSEMBLY BATCH (no serials) ---
        var subassemblyBatch = new List<ProductionLogFormDTO>
        {
            new(), new(), new()
        };

        var subassemblyResult = await service.SaveOrUpdateBatchAsync(
            subassemblyBatch, createdBy, operatorId, productId: 0, workInstructionId);

        Assert.Equal(3, subassemblyResult.CreatedCount);

        // Verify: none of the saved logs should have serial numbers
        await using (var verifyContext = new ApplicationContext(
                       new DbContextOptionsBuilder<ApplicationContext>()
                           .UseSqlite(CONNECTION_STRING)
                           .Options))
        {
            var logs = await verifyContext.ProductionLogs
                .Where(l => subassemblyResult.CreatedIds.Contains(l.Id))
                .ToListAsync();

            Assert.All(logs, l => Assert.True(string.IsNullOrEmpty(l.ProductSerialNumber)));
        }

        // --- FINAL ASSEMBLY BATCH (all with serials) ---
        var finalAssemblyBatch = new List<ProductionLogFormDTO>
        {
            new() { ProductSerialNumber = "FA-1001" },
            new() { ProductSerialNumber = "FA-1002" },
            new() { ProductSerialNumber = "FA-1003" }
        };

        var finalAssemblyResult = await service.SaveOrUpdateBatchAsync(
            finalAssemblyBatch, createdBy, operatorId, productId: 0, workInstructionId);

        Assert.Equal(3, finalAssemblyResult.CreatedCount);

        // Verify: all logs have correct serial numbers
        await using (var verifyContext2 = new ApplicationContext(
                       new DbContextOptionsBuilder<ApplicationContext>()
                           .UseSqlite(CONNECTION_STRING)
                           .Options))
        {
            var logs = await verifyContext2.ProductionLogs
                .Where(l => finalAssemblyResult.CreatedIds.Contains(l.Id))
                .ToListAsync();

            Assert.Equal(new[] { "FA-1001", "FA-1002", "FA-1003" },
                logs.Select(l => l.ProductSerialNumber).ToArray());
        }
    }
}