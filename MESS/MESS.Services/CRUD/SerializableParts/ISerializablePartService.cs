using MESS.Data.Models;

namespace MESS.Services.CRUD.SerializableParts;

/// <summary>
/// Defines a contract for performing CRUD operations on <see cref="SerializablePart"/> entities.
/// </summary>
public interface ISerializablePartService
{
    /// <summary>
    /// Creates a new <see cref="SerializablePart"/> record for the specified <see cref="PartDefinition"/>
    /// and serial number.
    /// </summary>
    /// <param name="definition">
    /// The <see cref="PartDefinition"/> associated with the serialized part.
    /// </param>
    /// <param name="serialNumber">
    /// The unique serial number identifying this specific part instance.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the newly created
    /// <see cref="SerializablePart"/> entity, or <see langword="null"/> if creation failed.
    /// </returns>
    Task<SerializablePart?> CreateAsync(PartDefinition definition, string serialNumber);

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