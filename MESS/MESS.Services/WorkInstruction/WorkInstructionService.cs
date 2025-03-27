using System.Reflection;
using ClosedXML.Excel;
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

    public WorkInstruction? ImportFromXlsx(string filePath)
    {
        try
        {
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);
        
            var workInstruction = new WorkInstruction
            {
                Title = worksheet.Cell("B1").GetString(),
                Version = worksheet.Cell("D1").GetString(),
                Steps = new List<Step>()
            };

            // Start from row 7 (assuming header row is 6)
            var stepStartRow = 7;
            while (!worksheet.Cell(stepStartRow, 1).IsEmpty())
            {
                var step = new Step
                {
                    Name = worksheet.Cell(stepStartRow, 2).GetString(),
                    Content = new List<string>(),
                    Body = worksheet.Cell(stepStartRow, 3).GetString(),
                    SubmitTime = DateTimeOffset.UtcNow,
                    PartsNeeded = new List<Part>(),
                    Success = false
                };
                
                var pictures = worksheet.Pictures
                    .Where(p => p.TopLeftCell.Address.RowNumber == stepStartRow 
                                && p.TopLeftCell.Address.ColumnNumber == 5)
                    .ToList();
                
                foreach (var picture in pictures)
                {
                    // Convert picture to base64 string
                    using var ms = new MemoryStream();
                    picture.ImageStream.CopyTo(ms);
                    var base64String = Convert.ToBase64String(ms.ToArray());
                    step.Content.Add($"data:image/png;base64,{base64String}");
                }
            
                workInstruction.Steps.Add(step);
                stepStartRow++;
            }

            if (Create(workInstruction))
            {
                Log.Information("Successfully imported WorkInstruction from Excel: {title}", workInstruction.Title);
                return workInstruction;
            }

            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to import WorkInstruction from Excel file: {filePath}", filePath);
            return null;
        }
    }

    public List<WorkInstruction> GetAll()
    {
        try
        {
            var workInstructions = _context.WorkInstructions
                .Include(w => w.Steps)
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
            if (id <= 0)
            {
                return null;
            }
            
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