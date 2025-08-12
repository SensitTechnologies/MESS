using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.ProductionLogPartService;

/// <inheritdoc />
public class ProductionLogPartService : IProductionLogPartService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductionLogPartService"/> class.
    /// </summary>
    /// <param name="contextFactory">The application database context used for data operations.</param>
    public ProductionLogPartService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public event Action? CurrentProductNumberChanged;
    private string? _currentProductNumber;
    
    /// <inheritdoc />
    public event Action? PartsReloadRequested;

    /// <inheritdoc />
    public void RequestPartsReload()
    {
        PartsReloadRequested?.Invoke();
    }

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

    private readonly Dictionary<int, LogPartEntryGroup> _logEntries = new();

    
    /// <inheritdoc />
    public async Task<bool> SaveAllLogPartsAsync(List<ProductionLog> savedLogs)
    {
        bool allSaved = true;

        foreach (var group in _logEntries.Values)
        {
            if (group.LogIndex < 0 || group.LogIndex >= savedLogs.Count)
            {
                Log.Error("Invalid log index {LogIndex} during SaveAllLogPartsAsync", group.LogIndex);
                allSaved = false;
                continue;
            }

            var productionLogId = savedLogs[group.LogIndex].Id;

            var allParts = group.GetAllParts()
                .Where(p => !string.IsNullOrWhiteSpace(p.PartSerialNumber))
                .ToList();

            foreach (var part in allParts)
            {
                part.ProductionLogId = productionLogId;
            }

            if (allParts.Count == 0)
                continue;

            var success = await CreateRangeAsync(allParts);
            if (!success)
            {
                Log.Warning("Failed to save parts for log at index {LogIndex}", group.LogIndex);
                allSaved = false;
            }
            else
            {
                Log.Information("Saved {Count} parts for log ID {LogId} (index {LogIndex})", allParts.Count, productionLogId, group.LogIndex);
            }
        }

        _logEntries.Clear();
        return allSaved;
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
    
    /// <inheritdoc />
    public void SetPartsForNode(int logIndex, int partNodeId, List<ProductionLogPart> parts)
    {
        if (!_logEntries.TryGetValue(logIndex, out var group))
        {
            group = new LogPartEntryGroup(logIndex);
            _logEntries[logIndex] = group;
        }

        group.SetPartsForNode(partNodeId, parts);
    }

    /// <inheritdoc />
    public List<ProductionLogPart> GetPartsForNode(int logIndex, int partNodeId)
    {
        return _logEntries.TryGetValue(logIndex, out var group)
            ? group.GetPartsForNode(partNodeId)
            : [];
    }

    /// <inheritdoc />
    public void ClearPartsForNode(int logIndex, int partNodeId)
    {
        if (_logEntries.TryGetValue(logIndex, out var group))
        {
            group.ClearPartsForNode(partNodeId);
        }
    }

    /// <inheritdoc />
    public void ClearAllLogParts()
    {
        _logEntries.Clear();
        Log.Information("Cleared all log parts.");
        RequestPartsReload();
    }
    
    /// <inheritdoc />
    public void EnsureRequiredPartsLogged(int logIndex, int partNodeId, List<Part> requiredParts)
    {
        var existingParts = GetPartsForNode(logIndex, partNodeId);

        var missingParts = requiredParts
            .Where(required => existingParts.All(existing => existing.Part?.Id != required.Id))
            .Select(p => new ProductionLogPart { Part = p })
            .ToList();

        if (missingParts.Count > 0)
        {
            existingParts.AddRange(missingParts);
            SetPartsForNode(logIndex, partNodeId, existingParts);
        }
    }
    
    /// <inheritdoc />
    public int GetTotalPartsLogged()
    {
        return _logEntries.Values
            .SelectMany(group => group.GetAllParts())
            .Count();
    }
}