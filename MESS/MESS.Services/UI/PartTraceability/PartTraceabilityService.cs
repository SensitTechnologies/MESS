using MESS.Data.Models;
using Serilog;

namespace MESS.Services.UI.PartTraceability;

/// <summary>
/// Manages the collection of <see cref="PartEntryGroup"/> instances used to track
/// part inputs across multiple production logs.
/// </summary>
public class PartTraceabilityService : IPartTraceabilityService
{
    /// <summary>
    /// Occurs when a full reload of part traceability data is requested.  
    /// </summary>
    /// <remarks>
    /// This event is typically raised after significant state changes, such as when all
    /// <see cref="PartEntryGroup"/> instances are cleared via <see cref="ClearAll"/>, 
    /// or when a manual refresh of part input data is needed in the UI.  
    /// Components or services that subscribe to this event should requery or refresh their
    /// displayed part data to stay in sync with the current application state.
    /// </remarks>
    public event Action? PartsReloadRequested;

    private readonly Dictionary<int, PartEntryGroup> _entryGroups = new();

    /// <inheritdoc />
    public void RequestPartsReload() => PartsReloadRequested?.Invoke();

    /// <summary>
    /// Gets or creates a <see cref="PartEntryGroup"/> for a given log index.
    /// </summary>
    private PartEntryGroup GetOrCreateGroup(int logIndex)
    {
        if (!_entryGroups.TryGetValue(logIndex, out var group))
        {
            group = new PartEntryGroup(logIndex);
            _entryGroups[logIndex] = group;
        }
        return group;
    }

    /// <summary>
    /// Sets a serializable part for the specified node in a given log index.
    /// </summary>
    public void SetSerializablePart(int logIndex, PartNode node, SerializablePart part)
    {
        GetOrCreateGroup(logIndex).SetSerializablePart(node, part);
    }

    /// <summary>
    /// Sets a linked production log for the specified node in a given log index.
    /// </summary>
    public void SetLinkedProductionLog(int logIndex, PartNode node, ProductionLog log)
    {
        GetOrCreateGroup(logIndex).SetLinkedProductionLog(node, log);
    }

    /// <summary>
    /// Clears all part/log entries for a specific node in a log group.
    /// </summary>
    public void ClearEntry(int logIndex, PartNode node)
    {
        if (_entryGroups.TryGetValue(logIndex, out var group))
            group.ClearEntry(node);
    }

    /// <summary>
    /// Clears all groups and requests a reload event.
    /// </summary>
    public void ClearAll()
    {
        _entryGroups.Clear();
        Log.Information("Cleared all part traceability groups.");
        RequestPartsReload();
    }

    /// <summary>
    /// Returns all inputs (serializable parts and linked logs) across all log groups.
    /// </summary>
    public IEnumerable<object> GetAllInputs()
    {
        return _entryGroups.Values.SelectMany(g => g.GetAllInputs());
    }
}
