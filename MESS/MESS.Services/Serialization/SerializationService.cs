using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.Serialization;

/// <inheritdoc />
public class SerializationService : ISerializationService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationService"/> class.
    /// </summary>
    /// <param name="contextFactory">The application database context used for data operations.</param>
    public SerializationService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public event Action? CurrentProductionLogPartChanged;
    /// <inheritdoc />
    public event Action? CurrentProductNumberChanged;
    private string? _currentProductNumber;

    /// <inheritdoc />
    public string? CurrentProductNumber
    {
        get => _currentProductNumber;
        set
        {
            _currentProductNumber = value;
            CurrentProductNumberChanged?.Invoke();
        }
    }

    private List<ProductionLogPart> _currentProductionLogParts = [];
    /// <inheritdoc />
    public List<ProductionLogPart> CurrentProductionLogParts
    {
        get => _currentProductionLogParts;
        set
        {
            _currentProductionLogParts = value;
            CurrentProductionLogPartChanged?.Invoke();
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveCurrentProductionLogPartsAsync(int productionLogId)
    {
        try
        {
            if (CurrentProductionLogParts.Count <= 0)
            {
                return false;
            }

            foreach (var log in CurrentProductionLogParts)
            {
                log.ProductionLogId = productionLogId;
            }

            var result = await CreateRangeAsync(CurrentProductionLogParts);
            if (result)
            {
                CurrentProductionLogParts.Clear();
            }
            return result;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to save Current Production Log Parts with ProductionLogID: {ID}. Exception Message {Message}", productionLogId, e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<ProductionLogPart>?> GetAllAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var logList = await context.ProductionLogParts
                .Include(l => l.Part)
                .ToListAsync();

            return logList;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to Get All ProductionLogParts Async: {ExceptionMessage}", e.Message);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateAsync(ProductionLogPart productionLogPart)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            await context.ProductionLogParts.AddAsync(productionLogPart);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to create ProductionLogPart Async: {ExceptionMessage}", e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateRangeAsync(List<ProductionLogPart> productionLogParts)
    {
        try
        {
            if (productionLogParts.Count <= 0)
            {
                Log.Warning("Attempted to add range of productionLogParts with {LogCount} logs", productionLogParts.Count);
                return false;
            }
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var serialNumberLog in productionLogParts)
            {
                if (serialNumberLog.Part is not null)
                {
                    context.Attach(serialNumberLog.Part);
                    context.Entry(serialNumberLog.Part).State = EntityState.Unchanged;
                }
            }

            await context.ProductionLogParts.AddRangeAsync(productionLogParts);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to Create a Range of ProductionLogParts with Exception Message: {ExceptionMessage}",  e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(ProductionLogPart productionLogPart)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.ProductionLogParts.Update(productionLogPart);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to update ProductionLogPart with ID: {ID} with Exception Message: {ExceptionMessage}", productionLogPart.Id, e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int serialNumberLogId)
    {
        try
        {
            if (serialNumberLogId <= 0)
            {
                Log.Error("Attempted to delete ProductionLogPart with invalid ID: {ID}", serialNumberLogId);
                return false;
            }
            await using var context = await _contextFactory.CreateDbContextAsync();

            var serialNumberLogToDelete = await context.ProductionLogParts.FindAsync(serialNumberLogId);

            if (serialNumberLogToDelete == null)
            {
                Log.Warning("Attempted to delete non-existent ProductionLogPart with ID: {ID}", serialNumberLogId);
                return false;
            }
            
            context.ProductionLogParts.Remove(serialNumberLogToDelete);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to delete productionLogPart with ID: {ID}: {ExceptionMessage}",serialNumberLogId, e.Message);
            return false;
        }
    }
}