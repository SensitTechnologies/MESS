using MESS.Data.Models;

namespace MESS.Services.UI.PartTraceability;

/// <summary>
/// Represents a collection of part node entries grouped under a specific production log index.
/// Each group contains multiple <see cref="PartEntry"/> objects, corresponding to UI part nodes.
/// </summary>
public class PartEntryGroup
{
    /// <summary>
    /// Gets or sets the index of the production log this group corresponds to.
    /// </summary>
    public int LogIndex { get; set; }

    /// <summary>
    /// Gets or sets the collection of part node entries within this group.
    /// </summary>
    public List<PartEntry> PartNodeEntries { get; set; } = [];
    
    /// <summary>
    /// If the associated WorkInstruction produces a part, this holds the
    /// serializable instance created in memory before saving.
    /// </summary>
    public SerializablePart? ProducedPart { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEntryGroup"/> class with the specified production log index.
    /// </summary>
    /// <param name="logIndex">The index of the production log that this group represents.</param>
    public PartEntryGroup(int logIndex)
    {
        LogIndex = logIndex;
    }

    /// <summary>
    /// Gets or creates a <see cref="PartEntry"/> for a given <see cref="PartNode"/>.
    /// </summary>
    private PartEntry GetOrCreateEntry(PartNode node)
    {
        var entry = PartNodeEntries.FirstOrDefault(e => e.PartNodeId == node.Id);
        if (entry != null) return entry;
        entry = new PartEntry(node);
        PartNodeEntries.Add(entry);
        return entry;
    }
    
    /// <summary>
    /// Returns the <see cref="SerializablePart"/> for a given node, if it exists.
    /// </summary>
    public SerializablePart? GetSerializablePart(PartNode node)
    {
        var entry = PartNodeEntries.FirstOrDefault(e => e.PartNodeId == node.Id);
        return entry?.SerializablePart;
    }
    
    /// <summary>
    /// Gets the serializable part for a node, creating a new placeholder if none exists.
    /// </summary>
    public SerializablePart GetOrCreateSerializablePart(PartNode node)
    {
        var entry = GetOrCreateEntry(node);
        if (entry.SerializablePart == null)
        {
            entry.SerializablePart = new SerializablePart
            {
                PartDefinitionId = node.PartDefinitionId,
                PartDefinition = node.PartDefinition,
                SerialNumber = null
            };
            entry.LinkedProductionLog = null;
        }
        return entry.SerializablePart;
    }
    
    /// <summary>
    /// Assigns a serializable part to a node whose input type expects a serial number.
    /// </summary>
    public void SetSerializablePart(PartNode node, SerializablePart part)
    {
        var entry = GetOrCreateEntry(node);
        if (node.InputType != PartInputType.SerialNumber)
            throw new InvalidOperationException($"Node {node.Id} does not accept serial number input.");
        entry.SerializablePart = part;
        entry.LinkedProductionLog = null;
    }

    /// <summary>
    /// Assigns a production log reference to a node whose input type expects a production log ID.
    /// </summary>
    public void SetLinkedProductionLog(PartNode node, ProductionLog productionLog)
    {
        var entry = GetOrCreateEntry(node);
        if (node.InputType != PartInputType.ProductionLogId)
            throw new InvalidOperationException($"Node {node.Id} does not accept production log input.");
        entry.LinkedProductionLog = productionLog;
        entry.SerializablePart = null;
    }

    /// <summary>
    /// Retrieves all active inputs (parts or logs) across this group.
    /// </summary>
    public IEnumerable<object> GetAllInputs()
    {
        foreach (var entry in PartNodeEntries)
        {
            if (entry.SerializablePart is not null)
                yield return entry.SerializablePart;
            else if (entry.LinkedProductionLog is not null)
                yield return entry.LinkedProductionLog;
        }
    }

    /// <summary>
    /// Clears the part/log entry for a specific node.
    /// </summary>
    public void ClearEntry(PartNode node)
    {
        PartNodeEntries.RemoveAll(e => e.PartNodeId == node.Id);
    }
}