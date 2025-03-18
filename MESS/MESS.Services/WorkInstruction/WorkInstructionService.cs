using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.WorkInstruction;
using MESS.Data.Models;
public class WorkInstructionService : IWorkInstructionService
{
    private readonly ApplicationContext _context;
    public WorkInstructionService(ApplicationContext context)
    {
        _context = context;
    }

    public List<WorkInstruction> GetAll()
    {
        try
        {
            var workInstructions = _context.WorkInstructions
                .Include(w => w.Steps)
                .Include(w => w.RelatedDocumentation)
                .ToList();

            return workInstructions;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetAll Work Instructions, in WorkInstructionService", e.GetBaseException().ToString());
            return [];
        }
    }

    public Task<List<WorkInstruction>> GetAllAsync()
    {
        try
        {
            var workInstructions = _context.WorkInstructions
                .Include(w => w.Steps)
                .ThenInclude(w => w.PartsNeeded)
                .Include(w => w.RelatedDocumentation)
                .ToListAsync();

            return workInstructions;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetAllAsync Work Instructions, in WorkInstructionService", e.GetBaseException().ToString());
            throw;
        }
    }

    public WorkInstruction? GetByTitle(string title)
    {
        try
        {
            var workInstruction = _context.WorkInstructions
                .FirstOrDefault(w => w.Title == title);

            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByTitle with Title: {title}, in WorkInstructionService", e.GetBaseException().ToString(), title);
            return null;
        }
    }

    public WorkInstruction? GetById(int id)
    {
        try
        {
            var workInstruction = _context.WorkInstructions.Find(id);

            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetById with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return null;
        }
    }

    public async Task<WorkInstruction?> GetByIdAsync(int id)
    {
        try
        {
            var workInstruction = await _context.WorkInstructions
                .Include(w => w.Steps)
                .ThenInclude(s => s.PartsNeeded)
                .FirstAsync(w => w.Id == id);
            
            return workInstruction;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByIdAsync with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return null;
        }
    }

    public bool Create(WorkInstruction workInstruction)
    {
        try
        {
            // Validate WorkInstruction
            var workInstructionValidator = new WorkInstructionValidator();
            var validationResult = workInstructionValidator.Validate(workInstruction);

            if (!validationResult.IsValid)
            {
                return false;
            }

            _context.WorkInstructions.Add(workInstruction);
            _context.SaveChanges();
            
            Log.Information("Successfully created WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to Create a work instruction, in WorkInstructionService", e.GetBaseException().ToString());
            return false;
        }
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        try
        {
            var workInstruction = await _context.WorkInstructions.FindAsync(id);
            
            if (workInstruction == null)
            {
                return false;
            }

            _context.WorkInstructions.Remove(workInstruction);
            await _context.SaveChangesAsync();
            
            Log.Information("Successfully deleted WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to Delete a work instruction with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), id);
            return false;
        }
    }

    public async Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction)
    {
        // if ID is 0 that means it has NOT been saved to the database
        if (workInstruction.Id == 0)
        {
            return false;
        }
        try
        {
            // verify that the WorkInstruction already exists in the database
            var existingWorkInstruction = await _context.WorkInstructions.FindAsync(workInstruction.Id);

            if (existingWorkInstruction == null)
            {
                return false;
            }
            
            Log.Information("Successfully updated WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            _context.WorkInstructions.Update(workInstruction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Log.Warning("Exception: {exceptionType} thrown when attempting to UpdateWorkInstructionAsync with ID: {id}, in WorkInstructionService", e.GetBaseException().ToString(), workInstruction.Id);
            return false;
        }
    }
}