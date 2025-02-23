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

    public async Task<LineOperator> UpdateLineOperator(LineOperator lineOperator) 
    {
        _context.LineOperators.Update(lineOperator);
        await _context.SaveChangesAsync();
        Log.Information("Updated LineOperator with ID {id}", lineOperator.Id);
        return lineOperator;
    }

    public async Task<bool> DeleteLineOperator(int id) 
    {
        var lineOperator = await _context.LineOperators.FindAsync(id);
        if (lineOperator == null)
        {
            return false;
        } 
        _context.LineOperators.Remove(lineOperator);
        await _context.SaveChangesAsync();
        Log.Information("Successfully deleted LineOperator with ID {id}", lineOperator.Id);
        return true;
        }
    }
