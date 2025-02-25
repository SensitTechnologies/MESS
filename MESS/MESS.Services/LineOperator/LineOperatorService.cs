using MESS.Data.Context;
using Serilog;

namespace MESS.Services.LineOperator;
using Data.Models;
using Microsoft.EntityFrameworkCore;

public class LineOperatorService : ILineOperatorService
{
    private readonly ApplicationContext _context;

    public LineOperatorService(ApplicationContext context)
    {
        _context = context;
    }

    public List<LineOperator> GetLineOperators()
    {
        return _context.LineOperators.ToList();
    }

    public LineOperator? GetLineOperatorById(int id)
    {
        var LineOperator = _context.LineOperators.Find(id);
        return LineOperator;
    }

    public LineOperator? GetLineOperatorByLastName(string lastName)
    {
        var LineOperator = _context.LineOperators.Find(lastName);
        return LineOperator;
    }

    public async Task<bool> AddLineOperator(LineOperator lineOperator)
    {
        try
        {
            var LineOperatorValidator = new LineOperatorValidator();
            var validationResult = LineOperatorValidator.Validate(lineOperator);

            if (!validationResult.IsValid)
            {
                return false;
            }

            await _context.LineOperators.AddAsync(lineOperator);
            await _context.SaveChangesAsync();
            Log.Information("Added LineOperator with ID {id}", lineOperator.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not add LineOperator");
            return false;
        }
    }

    public async Task<bool> UpdateLineOperator(LineOperator lineOperator)
    {
        var existingOperator = await _context.LineOperators.FindAsync(lineOperator.Id);
        if (existingOperator == null)
        {
            Log.Error("Could not find LineOperator with ID {id}", lineOperator.Id);
            return false;
        }

        existingOperator.FirstName = lineOperator.FirstName;
        existingOperator.LastName = lineOperator.LastName;

        await _context.SaveChangesAsync();
        Log.Information("Updated LineOperator with ID {id}", lineOperator.Id);
        return true;
    }

    public async Task<bool> DeleteLineOperator(int id)
    {
        var lineOperator = await _context.LineOperators.FindAsync(id);
        
        if (lineOperator == null)
        {
            return false; 
        }
        
        var relatedInstructions = await _context.WorkInstructions
            .Where(w => w.Operator != null && w.Operator.Id == id)  
            .ToListAsync();
        
        if (relatedInstructions != null && relatedInstructions.Any())
        {
            foreach (var instruction in relatedInstructions)
            {
                instruction.Operator = null;  
            }
        }
        await _context.SaveChangesAsync(); 

        _context.LineOperators.Remove(lineOperator);
        await _context.SaveChangesAsync(); 

        Log.Information("Successfully deleted LineOperator with ID {id}", lineOperator.Id);
        return true;
        }
    }
