using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.Serialization;

public class SerializationService : ISerializationService
{
    private readonly ApplicationContext _context;

    public SerializationService(ApplicationContext context)
    {
        _context = context;
    }
    
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