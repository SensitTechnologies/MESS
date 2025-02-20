using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.WorkStation;

using Data.Models;

public class WorkStationService : IWorkStationService
{
    private readonly ApplicationContext _context;

    public WorkStationService(ApplicationContext context)
    {
        _context = context;
    }
    
    public async Task AddWorkStationAsync(WorkStation workStation)
    {
        await _context.WorkStations.AddAsync(workStation);
        await _context.SaveChangesAsync();
    }

    public async Task<WorkStation?> FindWorkStationByIdAsync(int id)
    {
        try
        {
            var workStation = await _context.WorkStations
                .Include(p => p.Products)
                .FirstOrDefaultAsync(p => p.Id == id);

            return workStation;
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception occured while finding a work station by ID.");
            return null;
        }
    }

    public async Task<IEnumerable<WorkStation>> GetAllWorkStationsAsync()
    {
        try
        {
            return await _context.WorkStations
                .Include(p => p.Products)
                .ToListAsync();
        }
        catch(Exception e)
        {
            Log.Warning(e, "Exception occured while getting all work stations. \n" +
                           "Returning empty work station list.");
            return new List<WorkStation>();
        }
    }

    public async Task ModifyWorkStationAsync(WorkStation workStation)
    {
        try
        {
            _context.WorkStations.Update(workStation);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception occured while modifying a work station.");
        }
    }

    public async Task RemoveWorkStationAsync(int id)
    {
        var workStation = await _context.WorkStations.FindAsync(id);
        if (workStation != null)
        {
            _context.WorkStations.Remove(workStation);
            await _context.SaveChangesAsync();
        }
        else
        {
            Log.Warning("Work station for removal not found.");
        }
    }
}