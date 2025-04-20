namespace MESS.Services.ProductionLog;
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
    /// Retrieves a List of ProductionLog objects asynchronously from a list of IDs
    /// </summary>
    /// <param name="logIds">A list of integer production log ids</param>
    /// <returns>A List of Production Log object</returns>
    public Task<List<ProductionLog>?> GetProductionLogsByListOfIdsAsync(List<int> logIds);
}