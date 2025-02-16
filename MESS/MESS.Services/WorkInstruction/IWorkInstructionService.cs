namespace MESS.Services.WorkInstruction;
using Data.Models;

public interface IWorkInstructionService
{
    public List<WorkInstruction> GetAll();
    public Task<List<WorkInstruction>> GetAllAsync();

    public WorkInstruction? GetByTitle(string title);
    public WorkInstruction? GetById(int id);
    public Task<WorkInstruction?> GetByIdAsync(int id);

}