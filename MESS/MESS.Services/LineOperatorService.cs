using MESS.Data.Context;

namespace MESS.Services;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LineOperatorService
{
    private readonly ApplicationContext _context;

    public LineOperatorService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<List<LineOperator>> GetLineOperatorsAsync() // spits out a list of all line operators
    {
       return await _context.LineOperators.ToListAsync();
    }

    public async Task<LineOperator> AddLineOperatorAsync(LineOperator lineOperator) // adds a line operator with parameters
    {
       _context.LineOperators.Add(lineOperator);
       await _context.SaveChangesAsync();
       return lineOperator;
    }

    public async Task<LineOperator> UpdateLineOperator(LineOperator lineOperator) // updates a set line operator
    {
        _context.LineOperators.Update(lineOperator);
        await _context.SaveChangesAsync();
        return lineOperator;
    }

    public async Task DeleteLineOperator(int id) // deletes a line operator via id
    {
        var lineOperator = await _context.LineOperators.FindAsync(id);
        if (lineOperator != null)
        {
            _context.LineOperators.Remove(lineOperator);
            await _context.SaveChangesAsync();
        }
    }
}