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

    private readonly Dictionary<int, List<ProductionLogPart>> _partsByLogIndex = new();

    
    /// <inheritdoc />
    public async Task<bool> SaveAllLogPartsAsync(List<ProductionLog> savedLogs)
    {
        bool allSaved = true;

        foreach (var kvp in _partsByLogIndex)
        {
            int logIndex = kvp.Key;
            List<ProductionLogPart> parts = kvp.Value;

            if (logIndex < 0 || logIndex >= savedLogs.Count)
            {
                Log.Error("Invalid log index {LogIndex} during SaveAllLogPartsAsync", logIndex);
                allSaved = false;
                continue;
            }

            int productionLogId = savedLogs[logIndex].Id;

            // Filter out parts without a serial number
            var partsWithSerials = parts
                .Where(p => !string.IsNullOrWhiteSpace(p.PartSerialNumber))
                .ToList();

            if (partsWithSerials.Count == 0)
            {
                Log.Debug("No parts with serial numbers for log index {LogIndex}; skipping save.", logIndex);
                continue;
            }

            foreach (var part in partsWithSerials)
            {
                part.ProductionLogId = productionLogId;
            }

            var success = await CreateRangeAsync(partsWithSerials);
            if (!success)
            {
                Log.Warning("Failed to save parts for log at index {LogIndex}", logIndex);
                allSaved = false;
            }
            else
            {
                Log.Information("Saved {Count} parts for log ID {LogId} (index {LogIndex})", partsWithSerials.Count, productionLogId, logIndex);
            }
        }

        _partsByLogIndex.Clear();
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
    public List<ProductionLogPart> GetPartsForLogIndex(int logIndex)
    {
        if (_partsByLogIndex.TryGetValue(logIndex, out var parts))
        {
            Log.Debug("Retrieved {Count} parts for log index {Index}", parts.Count, logIndex);
            return parts;
        }

        Log.Debug("No parts found for log index {Index}", logIndex);
        return new();
    }

    /// <inheritdoc />
    public void SetPartsForLogIndex(int logIndex, List<ProductionLogPart> parts)
    {
        _partsByLogIndex[logIndex] = parts;
        Log.Debug("Set {Count} parts for log index {Index}", parts.Count, logIndex);
    }
    
    /// <inheritdoc />
    public void ClearPartsForLogIndex(int logIndex)
    {
        if (_partsByLogIndex.Remove(logIndex))
        {
            Log.Information("Cleared parts for production log at index {LogIndex}", logIndex);
        }
        else
        {
            Log.Debug("No parts found to clear for production log at index {LogIndex}", logIndex);
        }
    }
    
    /// <inheritdoc />
    public void ClearAllLogParts()
    {
        _partsByLogIndex.Clear();
        Log.Information("Cleared all log parts across all log indexes.");
        RequestPartsReload();
    }
    
}