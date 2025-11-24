using MESS.Data.Models;
using MESS.Services.DTOs;

namespace MESS.Services.CRUD.SerializableParts;

/// <summary>
/// Defines a contract for performing CRUD operations on <see cref="SerializablePart"/> entities.
/// </summary>
public interface ISerializablePartService
{
    /// <summary>
    /// Creates a new <see cref="SerializablePart"/> record for the specified 
    /// <see cref="PartDefinition"/>, using an optional serial number.
    /// </summary>
    /// <param name="definition">
    /// The <see cref="PartDefinition"/> associated with the serialized part.
    /// </param>
    /// <param name="serialNumber">
    /// The serial number identifying this specific part instance, or <c>null</c>
    /// /empty if the part does not have a serial number.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.  
    /// The task result contains the newly created <see cref="SerializablePart"/>,
    /// or <c>null</c> if creation failed.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Serial numbers are optional. Produced parts or early-stage parts may not
    /// yet have a serial number, and passing <c>null</c> or empty values is valid.
    /// </para>
    /// <para>
    /// If a non-null serial number is provided and a duplicate exists for the same
    /// <see cref="PartDefinition"/>, the duplicate is allowed; a warning may be logged,
    /// but the new record will still be created.
    /// </para>
    /// <para>
    /// The returned entity is detached from tracking after the context is disposed.
    /// </para>
    /// </remarks>
    Task<SerializablePart?> CreateAsync(PartDefinition definition, string? serialNumber);

    /// <summary>
    /// Retrieves a <see cref="SerializablePart"/> entity that matches the specified serial number.
    /// </summary>
    /// <param name="serialNumber">
    /// The serial number of the serialized part to retrieve.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the matching
    /// <see cref="SerializablePart"/> entity if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<SerializablePart?> GetBySerialNumberAsync(string serialNumber);

    /// <summary>
    /// Retrieves all <see cref="SerializablePart"/> entities associated with a specific <see cref="PartDefinition"/>.
    /// </summary>
    /// <param name="partDefinitionId">
    /// The unique identifier of the <see cref="PartDefinition"/>.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of
    /// <see cref="SerializablePart"/> entities. Returns an empty list if none exist.
    /// </returns>
    Task<List<SerializablePart>> GetByPartDefinitionIdAsync(int partDefinitionId);

    /// <summary>
    /// Retrieves all <see cref="SerializablePart"/> entities that were installed during a specified <see cref="ProductionLog"/> event.
    /// </summary>
    /// <param name="productionLogId">
    /// The unique identifier of the <see cref="ProductionLog"/> to filter by.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a list of installed
    /// <see cref="SerializablePart"/> entities. Returns an empty list if none exist.
    /// </returns>
    Task<List<SerializablePart>> GetInstalledForProductionLogAsync(int productionLogId);
    
    /// <summary>
    /// Retrieves all <see cref="SerializablePart"/> entities that were installed during the specified production logs,
    /// filtered to only include parts whose <see cref="PartDefinition.Id"/> exists in the provided set of expected part definitions.
    /// </summary>
    /// <param name="productionLogIds">
    /// A list of production log IDs to query. Each ID represents a prior production event to consider for installed parts.
    /// </param>
    /// <param name="expectedPartDefinitionIds">
    /// A set of <see cref="PartDefinition.Id"/> values representing the part definitions expected for the current work instruction.
    /// Only installed parts matching these definitions will be returned.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is a list of <see cref="InstalledPartResult"/> records,
    /// each containing the <see cref="SerializablePart"/> and its associated <c>ProductionLogId</c>.
    /// The order of results is not guaranteed.
    /// </returns>
    /// <remarks>
    /// This method is used to efficiently fetch only relevant installed parts from multiple production logs
    /// without performing a separate query per production log. It is optimized for batch loading of serializable parts
    /// to populate in-memory data structures for traceability or work instruction editing.
    /// </remarks>
    Task<List<InstalledPartResult>> GetInstalledForProductionLogsAsync(
        List<int> productionLogIds,
        HashSet<int> expectedPartDefinitionIds);
    
    /// <summary>
    /// Retrieves the first installed <see cref="SerializablePart"/> for a specific <see cref="ProductionLog"/>.
    /// </summary>
    /// <param name="productionLogId">The ID of the production log for which to retrieve the produced part.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is the first <see cref="SerializablePart"/>
    /// installed in the specified production log, or <c>null</c> if none exist.
    /// </returns>
    Task<SerializablePart?> GetProducedForProductionLogAsync(int productionLogId);

    /// <summary>
    /// Updates the serial number of an existing <see cref="SerializablePart"/>.
    /// </summary>
    /// <param name="serializablePartId">
    /// The unique identifier of the <see cref="SerializablePart"/> to update.
    /// </param>
    /// <param name="newSerialNumber">
    /// The new serial number to assign to the <see cref="SerializablePart"/>.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    Task UpdateSerialNumberAsync(int serializablePartId, string newSerialNumber);

    /// <summary>
    /// Checks whether a <see cref="SerializablePart"/> exists with the specified
    /// <see cref="PartDefinition"/> ID and serial number.
    /// </summary>
    /// <param name="partDefinitionId">
    /// The unique identifier of the <see cref="PartDefinition"/> to match.
    /// </param>
    /// <param name="serialNumber">
    /// The serial number to match.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is <c>true</c> if a matching
    /// <see cref="SerializablePart"/> exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(int partDefinitionId, string serialNumber);
}