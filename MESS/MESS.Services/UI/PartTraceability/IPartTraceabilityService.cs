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
    /// Requests a reload of all parts across tracked log groups, typically used to
    /// notify UI components that they should re-fetch data or refresh bindings.
    /// </summary>
    void RequestPartsReload();

    /// <summary>
    /// Assigns a <see cref="SerializablePart"/> to the specified <see cref="PartNode"/>
    /// within a given production log group.
    /// </summary>
    /// <param name="logIndex">The index of the production log group.</param>
    /// <param name="node">The part node associated with the part input.</param>
    /// <param name="part">The serializable part entered by the user or system.</param>
    void SetSerializablePart(int logIndex, PartNode node, SerializablePart part);

    /// <summary>
    /// Assigns a <see cref="ProductionLog"/> reference to the specified <see cref="PartNode"/>
    /// within a given production log group. This is typically used when the part input
    /// type is <see cref="PartInputType.ProductionLogId"/>.
    /// </summary>
    /// <param name="logIndex">The index of the production log group.</param>
    /// <param name="node">The part node associated with the linked log.</param>
    /// <param name="log">The production log that provides the linked part data.</param>
    void SetLinkedProductionLog(int logIndex, PartNode node, ProductionLog log);

    /// <summary>
    /// Clears any existing part or linked production log entry for a given node
    /// within a specific production log group.
    /// </summary>
    /// <param name="logIndex">The index of the production log group.</param>
    /// <param name="node">The part node whose entry should be cleared.</param>
    void ClearEntry(int logIndex, PartNode node);

    /// <summary>
    /// Removes all tracked part and log entries across all production log groups,
    /// and raises a <see cref="PartsReloadRequested"/> event to refresh dependent UI components.
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Retrieves all part-related inputs (either <see cref="SerializablePart"/> or
    /// <see cref="ProductionLog"/> objects) across all tracked log groups.
    /// </summary>
    /// <returns>
    /// An enumerable collection of all current part or log inputs across all groups.
    /// </returns>
    IEnumerable<object> GetAllInputs();
}