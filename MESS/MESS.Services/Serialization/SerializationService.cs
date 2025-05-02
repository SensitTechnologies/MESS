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
    public event Action? CurrentSerialNumberLogChanged;
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

    private List<SerialNumberLog> _currentSerialNumberLogs = [];
    /// <inheritdoc />
    public List<SerialNumberLog> CurrentSerialNumberLogs
    {
        get => _currentSerialNumberLogs;
        set
        {
            _currentSerialNumberLogs = value;
            CurrentSerialNumberLogChanged?.Invoke();
        }
    }

    /// <inheritdoc />
    public async Task<bool> SaveCurrentSerialNumberLogsAsync(int productionLogId)
    {
        try
        {
            if (CurrentSerialNumberLogs.Count <= 0)
            {
                return false;
            }

            foreach (var log in CurrentSerialNumberLogs)
            {
                log.ProductionLogId = productionLogId;
            }

            var result = await CreateRangeAsync(CurrentSerialNumberLogs);
            if (result)
            {
                CurrentSerialNumberLogs.Clear();
            }
            return result;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught when attempting to save Current Serial Number Logs with ProductionLogID: {ID}. Exception Message {Message}", productionLogId, e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<SerialNumberLog>?> GetAllAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var logList = await context.SerialNumberLogs
                .Include(l => l.Part)
                .ToListAsync();

            return logList;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to Get All SerialNumberLogs Async: {ExceptionMessage}", e.Message);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateAsync(SerialNumberLog serialNumberLog)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            await context.SerialNumberLogs.AddAsync(serialNumberLog);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to create SerialNumberLog Async: {ExceptionMessage}", e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateRangeAsync(List<SerialNumberLog> serialNumberLogs)
    {
        try
        {
            if (serialNumberLogs.Count <= 0)
            {
                Log.Warning("Attempted to add range of serialNumberLogs with {LogCount} logs", serialNumberLogs.Count);
                return false;
            }
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var serialNumberLog in serialNumberLogs)
            {
                if (serialNumberLog.Part is not null)
                {
                    context.Attach(serialNumberLog.Part);
                    context.Entry(serialNumberLog.Part).State = EntityState.Unchanged;
                }
            }

            await context.SerialNumberLogs.AddRangeAsync(serialNumberLogs);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to Create a Range of SerialNumberLogs with Exception Message: {ExceptionMessage}",  e.Message);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(SerialNumberLog serialNumberLog)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.SerialNumberLogs.Update(serialNumberLog);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to update SerialNumberLog with ID: {ID} with Exception Message: {ExceptionMessage}", serialNumberLog.Id, e.Message);
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
                Log.Error("Attempted to delete SerialNumberLog with invalid ID: {ID}", serialNumberLogId);
                return false;
            }
            await using var context = await _contextFactory.CreateDbContextAsync();

            var serialNumberLogToDelete = await context.SerialNumberLogs.FindAsync(serialNumberLogId);

            if (serialNumberLogToDelete == null)
            {
                Log.Warning("Attempted to delete non-existent SerialNumberLog with ID: {ID}", serialNumberLogId);
                return false;
            }
            
            context.SerialNumberLogs.Remove(serialNumberLogToDelete);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to delete serialNumberLog with ID: {ID}: {ExceptionMessage}",serialNumberLogId, e.Message);
            return false;
        }
    }
}