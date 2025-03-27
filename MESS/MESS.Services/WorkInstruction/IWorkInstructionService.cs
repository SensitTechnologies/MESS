using Microsoft.AspNetCore.Components.Forms;

namespace MESS.Services.WorkInstruction;
using Data.Models;

public interface IWorkInstructionService
{
    public Task<WorkInstruction?> ImportFromXlsx(List<IBrowserFile> files);
    /// <summary>
    /// Retrieves a List of WorkInstruction objects
    /// </summary>
    /// <returns>List of WorkInstruction objects</returns>
    public List<WorkInstruction> GetAll();
    /// <summary>
    /// Retrieves a List of WorkInstruction objects asynchronously
    /// </summary>
    /// <returns>List of WorkInstruction objects</returns>
    public Task<List<WorkInstruction>> GetAllAsync();
    /// <summary>
    /// Retrieves a WorkInstruction by its title.
    /// </summary>
    /// <param name="title">The title of the WorkInstruction to retrieve.</param>
    /// <returns>The WorkInstruction if found; otherwise, <c>null</c>.</returns>

    public WorkInstruction? GetByTitle(string title);
    /// <summary>
    /// Retrieves a WorkInstruction by its ID
    /// </summary>
    /// <param name="id">The ID of the WorkInstruction to retrieve.</param>
    /// <returns>The WorkInstruction if found; otherwise, <c>null</c>.</returns>
    public WorkInstruction? GetById(int id);
    /// <summary>
    /// Retrieves a WorkInstruction by its ID.
    /// </summary>
    /// <param name="id">The ID of the WorkInstruction to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the WorkInstruction if found; otherwise, <c>null</c>.</returns>
    public Task<WorkInstruction?> GetByIdAsync(int id);
    /// <summary>
    /// Creates a WorkInstruction object and saves it to the database.
    /// </summary>
    /// <param name="workInstruction">An instance of WorkInstruction.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> Create(WorkInstruction workInstruction);
    /// <summary>
    /// Deletes a WorkInstruction from the database.
    /// </summary>
    /// <param name="id">The ID of the desired WorkInstruction.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> DeleteByIdAsync(int id);
    /// <summary>
    /// Updates a WorkInstruction that is currently saved in the Database.
    /// </summary>
    /// <param name="workInstruction">A WorkInstruction instance.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction);

}