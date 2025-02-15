namespace MESS.Services.WorkInstruction;
using Data.Models;

public interface IWorkInstructionService
{
    public List<WorkInstruction> GetAll();
    public WorkInstruction? GetByTitle(string title);
    public WorkInstruction? GetById(int id);
}