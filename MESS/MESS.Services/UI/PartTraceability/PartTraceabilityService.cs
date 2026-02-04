using MESS.Data.Models;
using MESS.Services.CRUD.ProductionLogParts;
using MESS.Services.CRUD.SerializableParts;
using MESS.Services.DTOs.ProductionLogs.Form;
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
    private Dictionary<int, PartEntryGroup> _snapshotGroups = new();

    private readonly ISerializablePartService _serializablePartService;
    private readonly IProductionLogPartService _productionLogPartService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartTraceabilityService"/> class.
    /// </summary>
    /// <param name="serializablePartService">
    /// Service used to retrieve, create, and manage <see cref="SerializablePart"/> entities
    /// during persistence of part traceability data.
    /// </param>
    /// <param name="productionLogPartService">
    /// Service responsible for creating and managing <see cref="ProductionLogPart"/> records
    /// in the database.
    /// </param>
    /// <remarks>
    /// This constructor wires the service into the persistence layer by injecting the
    /// necessary CRUD services. The <see cref="ISerializablePartService"/> is used to resolve
    /// or create serialized part records, while the <see cref="IProductionLogPartService"/>
    /// handles database persistence of <see cref="ProductionLogPart"/> entries.
    /// </remarks>
    public PartTraceabilityService(
        ISerializablePartService serializablePartService,
        IProductionLogPartService productionLogPartService)
    {
        _serializablePartService = serializablePartService;
        _productionLogPartService = productionLogPartService;
    }
    
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
    public void SetProducedPart(int logIndex, PartDefinition partDefinition)
    {
        var group = GetOrCreateGroup(logIndex);

        group.ProducedPart = new SerializablePart
        {
            PartDefinition = partDefinition,
            PartDefinitionId = partDefinition.Id,
            SerialNumber = null // UI will fill this in OR auto-generate on save
        };
    }
    
    /// <inheritdoc />
    public void SetProducedPartSerialNumber(int logIndex, string? serialNumber)
    {
        var group = GetOrCreateGroup(logIndex);
        
        if (group.ProducedPart == null)
        {
            // There MUST be a produced part definition already selected
            Log.Warning("Attempted to set serial number before produced part was created for log index {LogIndex}.", logIndex);
            return;
        }

        group.ProducedPart.SerialNumber = string.IsNullOrWhiteSpace(serialNumber)
            ? null
            : serialNumber.Trim();
    }

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
        _snapshotGroups.Clear();
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
            : [];

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
    /// Outputs a detailed, human-readable representation of all tracked part entries in memory,
    /// including produced parts, installed serializable parts, linked production logs, and IDs.
    /// Useful for debugging to verify that all parts made it into the data structure.
    /// </summary>
    /// <returns>A formatted string with all part traceability information.</returns>
    public string DumpPartTraceability()
    {
        if (_entryGroups.Count == 0)
            return "No part traceability data available.";

        var sb = new System.Text.StringBuilder();

        foreach (var group in _entryGroups.Values.OrderBy(g => g.LogIndex))
        {
            sb.AppendLine($"--- Log Index: {group.LogIndex} ---");

            // Produced part (if any)
            if (group.ProducedPart != null)
            {
                sb.AppendLine(
                    $"[Produced Part] ID: {group.ProducedPart.Id}, Serial: {group.ProducedPart.SerialNumber ?? "N/A"}, " +
                    $"DefID: {group.ProducedPart.PartDefinitionId}, Name: {group.ProducedPart.PartDefinition?.Name ?? "N/A"}, " +
                    $"Number: {group.ProducedPart.PartDefinition?.Number ?? "N/A"}"
                );
            }
            else
            {
                sb.AppendLine("[Produced Part] None");
            }

            // PartNode entries
            foreach (var entry in group.PartNodeEntries.OrderBy(e => e.PartNode.Id))
            {
                var node = entry.PartNode;

                if (entry.SerializablePart != null)
                {
                    var part = entry.SerializablePart;
                    sb.AppendLine(
                        $"[Node Part] NodeID: {node.Id}, PartNodeName: {node.PartDefinition?.Name ?? "N/A"}, " +
                        $"PartID: {part.Id}, Serial: {part.SerialNumber ?? "N/A"}, " +
                        $"DefID: {part.PartDefinitionId}, Name: {part.PartDefinition?.Name ?? "N/A"}, " +
                        $"Number: {part.PartDefinition?.Number ?? "N/A"}"
                    );
                }
                else if (entry.LinkedProductionLog != null)
                {
                    var log = entry.LinkedProductionLog;
                    sb.AppendLine(
                        $"[Node Linked Log] NodeID: {node.Id}, LinkedLogID: {log.Id}, Product: {log.Product?.PartDefinition.Name ?? "N/A"}"
                    );
                }
                else
                {
                    sb.AppendLine($"[Node Empty] NodeID: {node.Id}, NodeName: {node.PartDefinition?.Name ?? "N/A"} has no serial or linked log.");
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Removes all traceability data associated with the specified production log index.
    /// This is used when the production log batch size decreases and a log is removed,
    /// ensuring no stale serializable part data remains in memory.
    /// </summary>
    /// <param name="logIndex">The log index to remove.</param>
    public void RemoveLogIndex(int logIndex)
    {
        if (_entryGroups.Remove(logIndex))
        {
            Log.Information("Removed part traceability data for log index {LogIndex}.", logIndex);
            RequestPartsReload();
        }
        else
        {
            Log.Debug("Attempted to remove part traceability for log index {LogIndex}, but no entry existed.", logIndex);
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> PersistAsync(List<ProductionLogFormDTO> savedLogs)
    {
        ArgumentNullException.ThrowIfNull(savedLogs);

        var partsToCreate = new List<ProductionLogPart>();
        var hasSnapshot = _snapshotGroups.Count > 0;

        foreach (var group in _entryGroups.Values)
        {
            var logIndex = group.LogIndex;

            if (logIndex < 0 || logIndex >= savedLogs.Count)
                continue;

            var savedLogId = savedLogs[logIndex].Id;
            if (savedLogId <= 0)
                continue;

            _snapshotGroups.TryGetValue(logIndex, out var snapshot);

            if (snapshot != null)
            {
                await IdentifySnapshotChangesAsync(
                    group,
                    snapshot,
                    savedLogId,
                    partsToCreate);
            }
            else
            {
                await IdentifyInitialInstallsAsync(
                    group,
                    savedLogId,
                    partsToCreate);
            }

            await ProcessProducedPartAsync(
                group,
                snapshot,
                savedLogId,
                partsToCreate);
        }

        if (partsToCreate.Count == 0)
            return true;

        try
        {
            return await _productionLogPartService.CreateRangeAsync(partsToCreate);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error persisting ProductionLogPart records.");
            return false;
        }
    }
    
    private async Task<SerializablePart?> GetOrPersistPartAsync(SerializablePart current)
    {
        if (current.Id > 0)
            return current;

        if (string.IsNullOrWhiteSpace(current.SerialNumber))
        {
            Log.Warning("Cannot persist SerializablePart because SerialNumber is null or empty.");
            return null;
        }

        if (current.PartDefinition == null)
        {
            Log.Warning("Cannot persist SerializablePart because PartDefinition is null.");
            return null;
        }

        try
        {
            var defId = current.PartDefinitionId;
            var serial = current.SerialNumber;

            if (await _serializablePartService.ExistsAsync(defId, serial))
            {
                return await _serializablePartService.GetBySerialNumberAsync(serial)
                       ?? await _serializablePartService.CreateAsync(current.PartDefinition, serial);
            }

            return await _serializablePartService.CreateAsync(current.PartDefinition, serial);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error resolving or creating SerializablePart for serial {Serial}.", current.SerialNumber);
            return null;
        }
    }
    
    private static ProductionLogPart CreateOp(
        int productionLogId,
        SerializablePart part,
        PartOperationType type)
    {
        return new ProductionLogPart
        {
            ProductionLogId = productionLogId,
            SerializablePartId = part.Id,
            SerializablePart = part,
            OperationType = type
        };
    }
    
    private async Task IdentifySnapshotChangesAsync(
        PartEntryGroup group,
        PartEntryGroup snapshot,
        int savedLogId,
        List<ProductionLogPart> partsToCreate)
    {
        var snapshotByNode = snapshot.PartNodeEntries
            .ToDictionary(e => e.PartNodeId);

        foreach (var entry in group.PartNodeEntries)
        {
            snapshotByNode.TryGetValue(entry.PartNodeId, out var snap);

            var currentSerial = entry.SerializablePart;
            var currentLinked = entry.LinkedProductionLog;

            var snapshotSerial = snap?.SerializablePart;
            var snapshotLinked = snap?.LinkedProductionLog;

            // Snapshot serial removed
            if (snapshotSerial != null && currentSerial == null && currentLinked == null)
            {
                partsToCreate.Add(CreateOp(savedLogId, snapshotSerial, PartOperationType.Removed));
                continue;
            }

            // Snapshot linked log removed
            if (snapshotLinked != null && currentSerial == null && currentLinked == null)
            {
                var produced = await _serializablePartService
                    .GetProducedForProductionLogAsync(snapshotLinked.Id);

                if (produced != null)
                    partsToCreate.Add(CreateOp(savedLogId, produced, PartOperationType.Removed));

                continue;
            }

            // Replacement: serial changed
            if (snapshotSerial != null &&
                currentSerial != null &&
                snapshotSerial.Id != currentSerial.Id)
            {
                partsToCreate.Add(CreateOp(savedLogId, snapshotSerial, PartOperationType.Removed));

                var installed = await GetOrPersistPartAsync(currentSerial);
                if (installed != null)
                    partsToCreate.Add(CreateOp(savedLogId, installed, PartOperationType.Installed));

                continue;
            }

            // Replacement: linked log changed
            if (snapshotLinked != null &&
                (currentLinked == null || currentLinked.Id != snapshotLinked.Id))
            {
                var produced = await _serializablePartService
                    .GetProducedForProductionLogAsync(snapshotLinked.Id);

                if (produced != null)
                    partsToCreate.Add(CreateOp(savedLogId, produced, PartOperationType.Removed));

                if (currentSerial != null)
                {
                    var installed = await GetOrPersistPartAsync(currentSerial);
                    if (installed != null)
                        partsToCreate.Add(CreateOp(savedLogId, installed, PartOperationType.Installed));
                }
            }
        }
    }
    
    private async Task IdentifyInitialInstallsAsync(
        PartEntryGroup group,
        int savedLogId,
        List<ProductionLogPart> partsToCreate)
    {
        foreach (var entry in group.PartNodeEntries)
        {
            SerializablePart? part = null;

            if (entry.SerializablePart != null)
            {
                part = await GetOrPersistPartAsync(entry.SerializablePart);
            }
            else if (entry.LinkedProductionLog != null)
            {
                part = await _serializablePartService
                    .GetProducedForProductionLogAsync(entry.LinkedProductionLog.Id);
            }

            var inputType = entry.PartNode?.InputType;

            if (inputType == PartInputType.SerialNumber &&
                (part == null || string.IsNullOrWhiteSpace(part.SerialNumber)))
                continue;

            if (inputType == PartInputType.ProductionLogId && part == null)
                continue;

            if (part != null)
            {
                partsToCreate.Add(CreateOp(savedLogId, part, PartOperationType.Installed));
            }
        }
    }
    
    private async Task ProcessProducedPartAsync(
        PartEntryGroup group,
        PartEntryGroup? snapshot,
        int savedLogId,
        List<ProductionLogPart> partsToCreate)
    {
        if (snapshot?.ProducedPart != null)
        {
            group.ProducedPart = snapshot.ProducedPart;
        }

        if (group.ProducedPart == null)
            return;

        var persisted = await GetOrPersistPartAsync(group.ProducedPart);
        if (persisted == null)
            return;

        partsToCreate.Add(CreateOp(savedLogId, persisted, PartOperationType.Produced));
    }
    
    /// <inheritdoc />
    public async Task LoadInstalledPartsIntoMemoryAsync(List<int> priorProductionLogIds)
    {
        _entryGroups.Clear();
        RequestPartsReload();

        if (priorProductionLogIds.Count == 0)
        {
            // No logs loaded → NO snapshot should exist
            _snapshotGroups.Clear();
            return;
        }

        // Map: ProductionLogId → LogIndex (dialog row order)
        var logIndexMap = priorProductionLogIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        // Compute expected part definition IDs from groups already in memory
        var expectedPartDefinitionIds = _entryGroups.Values
            .SelectMany(g => g.PartNodeEntries)
            .Select(e => e.PartNode.PartDefinitionId)
            .ToHashSet();

        // Fetch installed parts for prior logs filtered by expected part definitions
        var installedParts = await _serializablePartService
            .GetInstalledForProductionLogsAsync(
                priorProductionLogIds,
                expectedPartDefinitionIds);

        // Group: ProductionLogId → (PartDefinitionId → List<SerializablePart>)
        var groupedByLog = installedParts
            .GroupBy(r => r.ProductionLogId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(r => r.Part.PartDefinitionId)
                      .ToDictionary(
                          gg => gg.Key, 
                          gg => gg.Select(x => x.Part).ToList())
            );

        // Assign parts to the correct memory group based on dialog row order
        foreach (var productionLogId in priorProductionLogIds)
        {
            if (!logIndexMap.TryGetValue(productionLogId, out var logIndex))
                continue;

            var group = GetOrCreateGroup(logIndex);

            if (!groupedByLog.TryGetValue(productionLogId, out var defs))
                continue;

            // Match parts to nodes in this group
            foreach (var node in group.PartNodeEntries.Select(e => e.PartNode).OrderBy(n => n.Id))
            {
                if (!defs.TryGetValue(node.PartDefinitionId, out var list) || list.Count == 0)
                    continue;

                var part = list[0];
                list.RemoveAt(0);

                group.SetSerializablePart(node, part);
            }
        }
        
        CreateSnapshot();

        Log.Information(
            "Loaded {Count} serializable parts into traceability groups for prior logs {@Ids}.",
            installedParts.Count, priorProductionLogIds);
    }
    
    private void CreateSnapshot()
    {
        _snapshotGroups = _entryGroups.ToDictionary(
            x => x.Key,
            x => x.Value.Clone()
        );
    }
}