using MESS.Data.Models;

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
}
