using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;

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
                .ToList();

            return workInstructions;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
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
            return null;
        }
    }

    public Data.Models.WorkInstruction? GetById(int id)
    {
        throw new NotImplementedException();
    }
}