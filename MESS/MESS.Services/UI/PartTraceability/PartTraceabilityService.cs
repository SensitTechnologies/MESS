using MESS.Data.Models;
using Serilog;

namespace MESS.Services.UI.PartTraceability;

/// <summary>
/// Manages the collection of <see cref="PartEntryGroup"/> instances used to track
/// part inputs across multiple production logs.
/// </summary>
public class PartTraceabilityService : IPartTraceabilityService
{
    /// <inheritdoc />
    public event Action? PartsReloadRequested;

    private readonly Dictionary<int, PartEntryGroup> _entryGroups = new();

    /// <inheritdoc />
    public void RequestPartsReload() => PartsReloadRequested?.Invoke();

    private PartEntryGroup GetOrCreateGroup(int logIndex)
        => _entryGroups.TryGetValue(logIndex, out var group) 
            ? group 
            : (_entryGroups[logIndex] = new PartEntryGroup(logIndex));

    /// <inheritdoc />
    public void SetSerializablePart(int logIndex, PartNode node, SerializablePart part)
        => GetOrCreateGroup(logIndex).SetSerializablePart(node, part);

    /// <inheritdoc />
    public void SetLinkedProductionLog(int logIndex, PartNode node, ProductionLog log)
        => GetOrCreateGroup(logIndex).SetLinkedProductionLog(node, log);

    /// <inheritdoc />
    public void ClearEntry(int logIndex, PartNode node)
    {
        if (_entryGroups.TryGetValue(logIndex, out var group))
            group.ClearEntry(node);
    }

    /// <inheritdoc />
    public void ClearAll()
    {
        _entryGroups.Clear();
        Log.Information("Cleared all part traceability groups.");
        RequestPartsReload();
    }

    /// <inheritdoc />
    public IEnumerable<object> GetAllInputs()
        => _entryGroups.Values.SelectMany(g => g.GetAllInputs());

    /// <inheritdoc />
    public IEnumerable<object> GetInputsForLog(int logIndex)
        => _entryGroups.TryGetValue(logIndex, out var group) 
            ? group.GetAllInputs() 
            : Enumerable.Empty<object>();

    /// <inheritdoc />
    public IEnumerable<SerializablePart> GetAllSerializablePartsForLog(int logIndex)
        => GetInputsForLog(logIndex).OfType<SerializablePart>();

    /// <inheritdoc />
    public SerializablePart GetSerializablePart(int logIndex, PartNode node)
        => GetOrCreateGroup(logIndex).GetOrCreateSerializablePart(node);

    /// <summary>
    /// Optionally expose all groups for testing or iteration.
    /// </summary>
    public IReadOnlyCollection<PartEntryGroup> GetAllGroups() 
        => _entryGroups.Values.ToList().AsReadOnly();
}
