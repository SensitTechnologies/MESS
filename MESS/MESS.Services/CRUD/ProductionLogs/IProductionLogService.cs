using MESS.Services.DTOs.ProductionLogs.Batch;
using MESS.Services.DTOs.ProductionLogs.CreateRequest;
using MESS.Services.DTOs.ProductionLogs.Detail;
using MESS.Services.DTOs.ProductionLogs.Form;
using MESS.Services.DTOs.ProductionLogs.Summary;
using MESS.Services.DTOs.ProductionLogs.UpdateRequest;

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
    /// Retrieves all production logs in a lightweight form as <see cref="ProductionLogSummaryDTO"/> objects.
    /// This method performs a projection to avoid loading full entity graphs, returning only the summary data.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The result contains a list of <see cref="ProductionLogSummaryDTO"/> objects, one for each production log.
    /// If no logs exist, the list will be empty.
    /// </returns>
    Task<List<ProductionLogSummaryDTO>> GetAllSummariesAsync();

    /// <summary>
    /// Retrieves a single ProductionLog object asynchronously
    /// </summary>
    /// <param name="id">integer ID value</param>
    /// <returns>A nullable ProductionLog</returns>
    public Task<ProductionLog?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a single production log by its ID and maps it to a <see cref="ProductionLogDetailDTO"/>.
    /// EF will track the underlying entities so that step attempts can be updated via DTOs.
    /// </summary>
    /// <param name="id">The ID of the production log to retrieve.</param>
    /// <returns>
    /// A <see cref="ProductionLogDetailDTO"/> representing the log, or <c>null</c> if not found.
    /// </returns>
    Task<ProductionLogDetailDTO?> GetDetailByIdAsync(int id);
    
    /// <summary>
    /// Creates a new <see cref="ProductionLog"/> in the database using the provided <see cref="ProductionLogCreateRequest"/> DTO.
    /// </summary>
    /// <param name="request">The DTO containing data for the new production log, including steps and attempts.</param>
    /// <returns>
    /// A <see cref="Task{Int32}"/> representing the asynchronous operation.
    /// Returns the ID of the newly created production log if successful, or <c>-1</c> if creation failed.
    /// </returns>
    /// <remarks>
    /// This method only assigns foreign key IDs for related entities (Product and WorkInstruction) 
    /// to avoid EF Core tracking conflicts. Navigation properties are not attached.
    /// Steps and their attempts are created via EF Core cascade.
    /// </remarks>
    Task<int> CreateAsync(ProductionLogCreateRequest request);

    /// <summary>
    /// Updates an existing <see cref="ProductionLog"/> based on the specified <see cref="ProductionLogUpdateRequest"/> DTO.
    /// </summary>
    /// <param name="request">The DTO containing updated values for the production log, including attempts for existing steps.</param>
    /// <param name="modifiedBy">The identifier of the user performing the update, used for audit tracking.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> representing the asynchronous operation.
    /// Returns <c>true</c> if the update succeeded, or <c>false</c> if the log was not found or an error occurred.
    /// </returns>
    /// <remarks>
    /// Only existing <see cref="ProductionLogStep"/> attempts are updated. Steps themselves are not modified, 
    /// but the mapping supports future addition or removal of steps.  
    /// Foreign key relationships are maintained without attaching navigation properties to ensure EF Core tracking safety.  
    /// Only the necessary steps and attempts are loaded for efficient updates.
    /// </remarks>
    Task<bool> UpdateAsync(ProductionLogUpdateRequest request, string modifiedBy);
    
    /// <summary>
    /// Updates an existing production log using a <see cref="ProductionLogDetailDTO"/> as input.
    /// </summary>
    /// <param name="dto">
    /// The detail DTO containing the flattened step attempts and metadata for the log to update.
    /// </param>
    /// <returns>
    /// A task that resolves to <c>true</c> if the update succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method converts the flattened attempt structure in <paramref name="dto"/> into an
    /// update request grouped by log step. The <see cref="ProductionLogDetailDTO.LastModifiedBy"/>
    /// value is used when available; otherwise, the default user identifier "system" is applied.
    /// </remarks>
    Task<bool> UpdateDetailAsync(ProductionLogDetailDTO dto);

    /// <summary>
    /// Saves or updates a batch of production logs for a single operator.
    /// </summary>
    /// <param name="formDtos">The collection of <see cref="ProductionLogFormDTO"/> objects to save or update.</param>
    /// <param name="createdBy">Identifier of the user creating new logs.</param>
    /// <param name="operatorId">Identifier of the operator associated with the logs.</param>
    /// <param name="productId">The product ID associated with all logs in this batch.</param>
    /// <param name="workInstructionId">The work instruction ID associated with all logs in this batch.</param>
    /// <returns>A <see cref="ProductionLogBatchResult"/> containing counts and IDs of created and updated logs.</returns>
    Task<ProductionLogBatchResult> SaveOrUpdateBatchAsync(
        IEnumerable<ProductionLogFormDTO> formDtos,
        string createdBy,
        string operatorId,
        int productId,
        int workInstructionId);
    
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
    Task<bool> DeleteByWorkInstructionAsync(WorkInstruction workInstruction);

}