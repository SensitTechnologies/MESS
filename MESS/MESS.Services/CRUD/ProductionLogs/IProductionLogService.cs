namespace MESS.Services.CRUD.ProductionLogs;
using Data.Models;
/// <summary>
/// Interface for managing ProductionLog operations, including retrieval, creation, 
/// updating, and deletion of ProductionLog objects.
/// </summary>
public interface IProductionLogService
{
    /// <summary>
    ///  Retrieves a List of ProductionLog objects asynchronously
    /// </summary>
    /// <returns>List of ProductionLog objects</returns>
    public Task<List<ProductionLog>?> GetAllAsync();
    /// <summary>
    /// Retrieves a single ProductionLog object asynchronously
    /// </summary>
    /// <param name="id">integer ID value</param>
    /// <returns>A nullable ProductionLog</returns>
    public Task<ProductionLog?> GetByIdAsync(int id);
    /// <summary>
    /// Creates a new ProductionLog object and saves it to the database
    /// NOTE: Handles Audit related data
    /// </summary>
    /// <param name="productionLog">ProductionLog</param>
    /// <returns>int value indicating the logs ID if successful or a -1 if an exception is thrown</returns>
    public Task<int> CreateAsync(ProductionLog productionLog);

    /// <summary>
    /// Edits/Updates an existing ProductionLog object stored in the database Asynchronously
    /// </summary>
    /// <param name="existingProductionLog">The existing ProductionLog object</param>
    /// <returns>boolean success/failure value</returns>
    public Task<bool> UpdateAsync(ProductionLog existingProductionLog);
    /// <summary>
    /// Retrieves a List of ProductionLog objects asynchronously from a list of IDs
    /// </summary>
    /// <param name="logIds">A list of integer production log ids</param>
    /// <returns>A List of Production Log object</returns>
    public Task<List<ProductionLog>?> GetProductionLogsByListOfIdsAsync(List<int> logIds);
    
    /// <summary>
    /// Retrieves all production logs created by a specific operator (user) based on their unique OperatorId.
    /// Returns an empty list if none found.
    /// </summary>
    /// <param name="operatorId">The unique identifier of the operator (typically the ASP.NET Identity UserId).</param>
    /// <returns>A list of ProductionLog entities for the specified operator.</returns>
    Task<List<ProductionLog>?> GetProductionLogsByOperatorIdAsync(string operatorId);

    /// <summary>
    /// Deletes a <see cref="ProductionLogStepAttempt"/> from the database by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the attempt to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAttemptAsync(int id);
    
    /// <summary>
    /// Deletes the specified production log and all associated log steps and step attempts from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the production log to delete.</param>
    /// <returns>
    /// A task that represents the asynchronous delete operation.
    /// The task result contains <c>true</c> if the log was found and successfully deleted; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> DeleteProductionLogAsync(int id);

    /// <summary>
    /// Deletes a specified production log and all associated log steps and attempts from the database.
    /// </summary>
    /// <param name="workInstruction">The work instruction object to delete from</param>
    /// <returns>
    /// A task that represents the asynchronous delete operation.
    /// The task result contains <c>true</c> if the log was found and successfully deleted; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> DeleteProductionLogByWorkInstructionAsync(WorkInstruction workInstruction);

}