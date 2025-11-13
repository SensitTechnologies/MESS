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
    
    /// <summary>
    /// Gets the total number of serializable parts that have a populated serial number
    /// across all production logs. This reflects parts that were installed or created
    /// and tracked in memory.
    /// </summary>
    /// <returns>The count of logged serializable parts.</returns>
    public int GetTotalPartsLogged()
    {
        return _entryGroups.Values
            .SelectMany(group => group.GetAllInputs().OfType<SerializablePart>())
            .Count(part => !string.IsNullOrWhiteSpace(part.SerialNumber));
    }
    
    /// <summary>
    /// Outputs a human-readable representation of all tracked part entries in memory.
    /// Useful for debugging to inspect which serializable parts are logged,
    /// their serial numbers, part names, part numbers, and linked production logs if applicable.
    /// </summary>
    /// <returns>A string containing the formatted representation of all entries.</returns>
    public string DumpPartTraceability()
    {
        if (_entryGroups.Count == 0)
            return "No part traceability data available.";

        var sb = new System.Text.StringBuilder();

        foreach (var group in _entryGroups.Values.OrderBy(g => g.LogIndex))
        {
            sb.AppendLine($"--- Log Index: {group.LogIndex} ---");

            foreach (var input in group.GetAllInputs())
            {
                switch (input)
                {
                    case SerializablePart part:
                        sb.AppendLine($"Serial: {part.SerialNumber ?? "N/A"}, " +
                                      $"Name: {part.PartDefinition?.Name ?? "N/A"}, " +
                                      $"Number: {part.PartDefinition?.Number ?? "N/A"}");
                        break;
                    case ProductionLog log:
                        sb.AppendLine($"Linked Production Log ID: {log.Id}, Product: {log.Product?.PartDefinition.Name ?? "N/A"}");
                        break;
                    default:
                        sb.AppendLine($"Unknown input type: {input.GetType().Name}");
                        break;
                }
            }
        }

        return sb.ToString();
    }

}
