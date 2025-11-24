using MESS.Data.Models;
using MESS.Services.CRUD.ProductionLogParts;
using MESS.Services.CRUD.SerializableParts;
using MESS.Services.DTOs.ProductionLogs.Form;

namespace MESS.Services.UI.PartTraceability;

/// <summary>
/// Provides an interface for managing part traceability input state across multiple
/// production logs and their associated <see cref="PartNode"/> entries.
/// </summary>
/// <remarks>
/// This service maintains in-memory collections of <see cref="PartEntryGroup"/> instances,
/// each corresponding to a specific production log index. It is primarily used to
/// track user-entered part data before it is persisted to the database.
/// </remarks>
public interface IPartTraceabilityService
{
    /// <summary>
    /// Occurs when part data has been reloaded or cleared, prompting dependent UI
    /// components to refresh their displayed state.
    /// </summary>
    event Action? PartsReloadRequested;

    /// <summary>
    /// Requests a reload of all parts across tracked log groups.
    /// Typically used to notify UI components to re-fetch or refresh bindings.
    /// </summary>
    void RequestPartsReload();

    /// <summary>
    /// Registers the part produced by the work instruction for the specified
    /// production log index.  
    /// </summary>
    /// <remarks>
    /// Many work instructions optionally define a <see cref="PartDefinition"/> that
    /// represents a new serialized part created during execution of the production log.
    /// This method prepares the produced part in the in-memory traceability structure so
    /// that it can be assigned a serial number (or no serial number)
    /// and persisted during <c>PersistAsync</c>.
    ///
    /// This applies to the *produced* part for the log as a whole, not to parts
    /// installed via <see cref="PartNode"/> entries.
    /// </remarks>
    /// <param name="logIndex">
    /// The index of the production log within the current batch.
    /// </param>
    /// <param name="partDefinition">
    /// The definition of the part that the work instruction produces.
    /// </param>
    void SetProducedPart(int logIndex, PartDefinition partDefinition);
    
    /// <summary>
    /// Records a produced serialized part for the specified production log index.
    /// </summary>
    /// <param name="logIndex">
    /// The zero-based index of the production log within the active batch for which
    /// the part was produced.
    /// </param>
    /// <param name="serialNumber">
    /// The serial number of the part that was produced. This value is stored
    /// immediately in the traceability service's in-memory structure.
    /// </param>
    /// <remarks>
    /// This method should be called as soon as the user enters a produced part
    /// serial number in the UI. It does not write to the database directly; instead,
    /// the value is captured in memory and later persisted when <see cref="PersistAsync"/>
    /// is executed.
    /// </remarks>
    void SetProducedPartSerialNumber(int logIndex, string? serialNumber);

    /// <summary>
    /// Assigns a <see cref="SerializablePart"/> to the specified <see cref="PartNode"/>
    /// within a given production log group.
    /// </summary>
    void SetSerializablePart(int logIndex, PartNode node, SerializablePart part);

    /// <summary>
    /// Assigns a <see cref="ProductionLog"/> reference to the specified <see cref="PartNode"/>
    /// within a given production log group.
    /// </summary>
    void SetLinkedProductionLog(int logIndex, PartNode node, ProductionLog log);

    /// <summary>
    /// Clears any existing part or linked production log entry for a given node
    /// within a specific production log group.
    /// </summary>
    void ClearEntry(int logIndex, PartNode node);

    /// <summary>
    /// Removes all tracked part and log entries across all production log groups,
    /// and raises <see cref="PartsReloadRequested"/> to refresh dependent UI components.
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Retrieves all part-related inputs (either <see cref="SerializablePart"/> or
    /// <see cref="ProductionLog"/> objects) across all tracked log groups.
    /// </summary>
    IEnumerable<object> GetAllInputs();

    /// <summary>
    /// Returns all part-related inputs (serializable parts and linked logs) for a specific log index.
    /// </summary>
    IEnumerable<object> GetInputsForLog(int logIndex);

    /// <summary>
    /// Returns only serializable parts for a specific log index.
    /// </summary>
    IEnumerable<SerializablePart> GetAllSerializablePartsForLog(int logIndex);

    /// <summary>
    /// Gets the <see cref="SerializablePart"/> for a given log index and node.
    /// Creates a new placeholder if none exists.
    /// </summary>
    SerializablePart GetSerializablePart(int logIndex, PartNode node);

    /// <summary>
    /// Optional: returns all <see cref="PartEntryGroup"/> instances for inspection or iteration.
    /// </summary>
    IReadOnlyCollection<PartEntryGroup> GetAllGroups();
    
    /// <summary>
    /// Gets the total number of serializable parts that have been logged across all production logs.
    /// Only parts that have a populated <see cref="SerializablePart.SerialNumber"/> are counted.
    /// This reflects parts that were either previously produced and installed or newly created and installed.
    /// </summary>
    /// <returns>The total count of logged serializable parts.</returns>
    int GetTotalPartsLogged();
    
    /// <summary>
    /// Generates a human-readable string representing all currently tracked part entries
    /// in memory, including serial numbers, part names, and any linked production logs.
    /// Useful for debugging the state of the in-memory part traceability data structure.
    /// </summary>
    /// <returns>A formatted string containing all part traceability entries for inspection.</returns>
    string DumpPartTraceability();
    
    /// <summary>
    /// Removes all part traceability entries belonging to the specified production log index.
    /// Call this when the batch size is reduced and a log at this index no longer exists.
    /// </summary>
    /// <param name="logIndex">The log index whose data should be removed.</param>
    void RemoveLogIndex(int logIndex);
    
    /// <summary>
    /// Persists the in-memory part traceability entries to the database using the provided
    /// saved production logs to determine the final <see cref="ProductionLog"/> IDs.
    /// </summary>
    /// <param name="savedLogs">
    /// A list of <see cref="ProductionLogFormDTO"/> instances that have already been
    /// persisted and whose <see cref="ProductionLogFormDTO.Id"/> values are the database IDs
    /// for the corresponding log indices (index -> savedLogs[index]).
    /// </param>
    /// <returns>
    /// A task that returns <c>true</c> when persistence succeeds (ProductionLogPart records created),
    /// otherwise <c>false</c>. The method will attempt to create any missing <see cref="SerializablePart"/>
    /// records as needed (using <see cref="ISerializablePartService"/>), and then create the
    /// corresponding <see cref="ProductionLogPart"/> entries (using <see cref="IProductionLogPartService"/>).
    /// </returns>
    Task<bool> PersistAsync(List<ProductionLogFormDTO> savedLogs);
    
    /// <summary>
    /// Loads installed serializable parts from the specified prior production logs into memory.
    /// </summary>
    /// <param name="priorProductionLogIds">
    /// A list of production log IDs representing previously saved logs for which installed parts
    /// should be retrieved and mapped into in-memory <see cref="PartEntryGroup"/> instances.
    /// </param>
    /// <remarks>
    /// This method clears any existing part traceability groups in memory before loading.
    /// 
    /// The method automatically determines the expected <see cref="PartDefinition"/> IDs based
    /// on the <see cref="PartNode"/> entries already present in the in-memory groups. It then
    /// fetches installed parts corresponding to those part definitions and assigns them to the
    /// correct part nodes within each production log group.
    ///
    /// This eliminates the need to pass in part nodes explicitly, simplifying the API.
    /// 
    /// The order of <paramref name="priorProductionLogIds"/> is used to determine the log index
    /// mapping in memory (e.g., row order in the dialog or UI).
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LoadInstalledPartsIntoMemoryAsync(List<int> priorProductionLogIds);
}
