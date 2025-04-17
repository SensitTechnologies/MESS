using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.Serialization;

/// <inheritdoc />
public class SerializationService : ISerializationService
{
    private readonly ApplicationContext _context;
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationService"/> class.
    /// </summary>
    /// <param name="context">The application database context used for data operations.</param>
    public SerializationService(ApplicationContext context)
    {
        _context = context;
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
            var logList = await _context.SerialNumberLogs
                .Include(l => l.Part)
                .ToListAsync();

            return logList;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception caught while attempting to Get All SerialNumberLogs Async: {ExceptionMessage}", e.Message);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateAsync(SerialNumberLog serialNumberLog)
    {
        try
        {
            await _context.SerialNumberLogs.AddAsync(serialNumberLog);
            await _context.SaveChangesAsync();
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

            await _context.SerialNumberLogs.AddRangeAsync(serialNumberLogs);
            await _context.SaveChangesAsync();

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
            _context.SerialNumberLogs.Update(serialNumberLog);
            await _context.SaveChangesAsync();
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

            var serialNumberLogToDelete = await _context.SerialNumberLogs.FindAsync(serialNumberLogId);

            if (serialNumberLogToDelete == null)
            {
                Log.Warning("Attempted to delete non-existent SerialNumberLog with ID: {ID}", serialNumberLogId);
                return false;
            }
            
            _context.SerialNumberLogs.Remove(serialNumberLogToDelete);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while attempting to delete serialNumberLog with ID: {ID}: {ExceptionMessage}",serialNumberLogId, e.Message);
            return false;
        }
    }
}