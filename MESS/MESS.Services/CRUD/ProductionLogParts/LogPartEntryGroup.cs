using MESS.Data.Models;

namespace MESS.Services.CRUD.ProductionLogParts;

/// <summary>
/// Represents a collection of production log part entries grouped under a specific production log index.
/// Each group contains multiple <see cref="PartNodeLogEntry"/> items corresponding to different <see cref="PartNode"/>s.
/// </summary>
public class LogPartEntryGroup
{
    /// <summary>
    /// Gets or sets the index of the production log this group corresponds to.
    /// </summary>
    public int LogIndex { get; set; }
    
    /// <summary>
    /// Gets or sets the list of <see cref="PartNodeLogEntry"/> objects associated with this log index.
    /// Each entry represents a single node's part log entries.
    /// </summary>
    public List<PartNodeLogEntry> NodeEntries { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LogPartEntryGroup"/> class with the specified production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log this group represents.</param>
    public LogPartEntryGroup(int logIndex)
    {
        LogIndex = logIndex;
    }

    /// <summary>
    /// Sets or replaces the list of <see cref="ProductionLogPart"/> entries for the specified <see cref="PartNode"/>.
    /// </summary>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to assign the parts to.</param>
    /// <param name="parts">The list of <see cref="ProductionLogPart"/> entries to associate with the node.</param>
    public void SetPartsForNode(int partNodeId, List<ProductionLogPart> parts)
    {
        var entry = NodeEntries.FirstOrDefault(e => e.PartNodeId == partNodeId);
        if (entry == null)
        {
            entry = new PartNodeLogEntry(partNodeId);
            NodeEntries.Add(entry);
        }

        entry.LogParts = parts;
    }

    /// <summary>
    /// Retrieves the list of <see cref="ProductionLogPart"/> entries associated with the specified <see cref="PartNode"/>.
    /// </summary>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to retrieve parts for.</param>
    /// <returns>A list of <see cref="ProductionLogPart"/> entries, or an empty list if none are found.</returns>
    public List<ProductionLogPart> GetPartsForNode(int partNodeId)
    {
        return NodeEntries.FirstOrDefault(e => e.PartNodeId == partNodeId)?.LogParts ?? [];
    }

    /// <summary>
    /// Clears all <see cref="ProductionLogPart"/> entries associated with the specified <see cref="PartNode"/>.
    /// </summary>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> to clear parts from.</param>
    public void ClearPartsForNode(int partNodeId)
    {
        NodeEntries.RemoveAll(e => e.PartNodeId == partNodeId);
    }

    /// <summary>
    /// Retrieves a flattened list of all <see cref="ProductionLogPart"/> entries across all part nodes in this group.
    /// </summary>
    /// <returns>A list of all <see cref="ProductionLogPart"/> entries in the group.</returns>
    public List<ProductionLogPart> GetAllParts() =>
        NodeEntries.SelectMany(n => n.LogParts).ToList();
}