namespace MESS.Services.ProductionLog;
using Data.Models;
public interface IProductionLogService
{
    /// <summary>
    /// Retrieves a List of ProductionLog objects
    /// </summary>
    /// <returns>List of ProductionLog objects </returns>
    /// <returns>OR an empty List if the operation failed</returns>
    public List<ProductionLog> GetAll();
    /// <summary>
    ///  Retrieves a List of ProductionLog objects asynchronously
    /// </summary>
    /// <returns>List of ProductionLog objects</returns>
    public Task<List<ProductionLog>?> GetAllAsync();
    /// <summary>
    /// Determines the Total Time for a given work instruction instance for a Production Log.
    /// Calculates total time through the summation of its ProductionLogStep's duration.
    /// </summary>
    /// <param name="log">A ProductionLog object</param>
    /// <returns>A nullable TimeSpan object</returns>
    public TimeSpan? GetTotalTime(ProductionLog log);
    /// <summary>
    /// Retrieves a single ProductionLog object
    /// </summary>
    /// <param name="id">integer ID value</param>
    /// <returns>A nullable ProductionLog</returns>
    public ProductionLog? GetById(int id);
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
    /// <returns>bool value indicating if the operation was successful</returns>
    public bool Create(ProductionLog productionLog);
    /// <summary>
    /// Deletes an existing ProductionLog object from the database
    /// </summary>
    /// <param name="id">integer ID value</param>
    /// <returns>bool value indicating if the operation was successful</returns>
    public bool Delete(int id);
    /// <summary>
    /// Edits/Updates an existing ProductionLog object stored in the database
    /// </summary>
    /// <param name="existing">The pre-existing ProductionLog object stored in the database</param>
    /// <param name="updated">The new ProductionLog object to replace the 'existing' object</param>
    /// <returns></returns>
    public ProductionLog EditWithObject(ProductionLog existing, ProductionLog updated);

    /// <summary>
    /// Edits/Updates an existing ProductionLog object stored in the database Asynchronously
    /// </summary>
    /// <param name="existingProductionLog">The existing ProductionLog object</param>
    /// <returns>boolean success/failure value</returns>
    public Task<bool> UpdateAsync(ProductionLog existingProductionLog);
}