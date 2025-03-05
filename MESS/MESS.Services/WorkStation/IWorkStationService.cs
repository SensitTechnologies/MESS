namespace MESS.Services.WorkStation;

using MESS.Data.Models;

public interface IWorkStationService
{
    /// <summary>
    /// Adds a new Work Station to the database.
    /// </summary>
    /// <param name="workStation">The product to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddWorkStationAsync(Data.Models.WorkStation workStation);
    
    /// <summary>
    /// Finds a WorkStation by its unique ID, including related Products.
    /// </summary>
    /// <param name="id">The ID of the WorkStation to find.</param>
    /// <returns>The WorkStation if found; otherwise, null.</returns>
    Task<WorkStation?> FindWorkStationByIdAsync(int id);
    
    /// <summary>
    /// Retrieves all WorkStations from the database, including related Products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    Task<IEnumerable<WorkStation>> GetAllWorkStationsAsync();
    
    /// <summary>
    /// Updates an existing WorkStation in the database.
    /// </summary>
    /// <param name="workStation">The WorkStation to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ModifyWorkStationAsync(WorkStation workStation);
    
    /// <summary>
    /// Removes a WorkStation from the database by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the WorkStation to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveWorkStationAsync(int id);
}