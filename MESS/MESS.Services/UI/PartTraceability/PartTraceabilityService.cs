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
    
    /// <summary>
    /// Persists in-memory part traceability entries into the database.
    /// For each group (logIndex) the corresponding savedLogs[logIndex].Id is used
    /// as the ProductionLogId for created ProductionLogPart records.
    /// </summary>
    public async Task<bool> PersistAsync(List<ProductionLogFormDTO> savedLogs)
    {
        ArgumentNullException.ThrowIfNull(savedLogs);

        var partsToCreate = new List<ProductionLogPart>();

        // Determine whether we have a snapshot (Scenario 2) or not (Scenario 1)
        var hasSnapshot = _snapshotGroups.Count > 0;

        // Iterate through current groups (these are the logs we're saving)
        foreach (var group in _entryGroups.Values)
        {
            var logIndex = group.LogIndex;

            // Validate mapping to savedLogs
            if (logIndex < 0 || logIndex >= savedLogs.Count)
            {
                Log.Warning("No saved production log for log index {LogIndex}; skipping persistence for this group.", logIndex);
                continue;
            }

            var savedLogId = savedLogs[logIndex].Id;
            if (savedLogId <= 0)
            {
                Log.Warning("Saved production log at index {LogIndex} has invalid Id {Id}; skipping group.", logIndex, savedLogId);
                continue;
            }

            // find snapshot group if present
            _snapshotGroups.TryGetValue(logIndex, out var snapshotGroup);

            // Build quick lookup of snapshot entries by PartNodeId
            var snapshotEntriesByNode = (snapshotGroup?.PartNodeEntries ?? [])
                .ToDictionary(e => e.PartNodeId, e => e);

            // For each entry in current group, decide actions
            foreach (var entry in group.PartNodeEntries)
            {
                var currentSerial = entry.SerializablePart;
                var currentLinkedLog = entry.LinkedProductionLog;

                snapshotEntriesByNode.TryGetValue(entry.PartNodeId, out var snapshotEntry);
                var snapshotSerial = snapshotEntry?.SerializablePart;
                var snapshotLinked = snapshotEntry?.LinkedProductionLog;

                // ---------- SNAPSHOT MODE ----------
                if (hasSnapshot && snapshotEntry != null)
                {
                    // 1) Snapshot had a serial and current has no entry => REMOVED
                    if (snapshotSerial != null && currentSerial == null && currentLinkedLog == null)
                    {
                        // Removed: use snapshotSerial.Id
                        partsToCreate.Add(BuildRemovedPart(savedLogId, snapshotSerial));
                        Log.Information("Detected removal (snapshot serial existed, now empty) for node {NodeId} in logIndex {LogIndex}.", entry.PartNodeId, logIndex);
                        continue;
                    }

                    // 2) Snapshot had a linked production log and current is now empty => REMOVED (resolve produced via linked)
                    if (snapshotLinked != null && currentSerial == null && currentLinkedLog == null)
                    {
                        try
                        {
                            var produced = await _serializablePartService.GetProducedForProductionLogAsync(snapshotLinked.Id);
                            if (produced != null)
                            {
                                partsToCreate.Add(BuildRemovedPart(savedLogId, produced));
                                Log.Information("Detected removal of part produced in prior log {PriorLogId} for node {NodeId} in logIndex {LogIndex}.", snapshotLinked.Id, entry.PartNodeId, logIndex);
                            }
                            else
                            {
                                Log.Warning("Snapshot linked log {PriorLogId} could not be resolved to a produced part; cannot produce removal entry.", snapshotLinked.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Error resolving produced part for linked snapshot log {LinkedLogId}.", snapshotLinked.Id);
                        }

                        continue;
                    }

                    // 3) Snapshot had a serial and current has a different serial => Removed(old) + Installed(new)
                    if (snapshotSerial != null && currentSerial != null && snapshotSerial.Id != currentSerial.Id)
                    {
                        // Removed old
                        partsToCreate.Add(BuildRemovedPart(savedLogId, snapshotSerial));
                        Log.Information("Replacing serial for node {NodeId} in logIndex {LogIndex}: removed old serial {OldSerial}.", entry.PartNodeId, logIndex, snapshotSerial.SerialNumber);

                        // Installed new — but ensure we have a persisted SerializablePart (attempt reuse if same serial exists)
                        var installedPart = currentSerial;
                        if (currentSerial.Id <= 0)
                        {
                            // The currentSerial in memory may not be in DB; attempt to check/create
                            try
                            {
                                // If serial string provided, try to create or fetch
                                if (!string.IsNullOrWhiteSpace(currentSerial.SerialNumber))
                                {
                                    var serialStr = currentSerial.SerialNumber!;
                                    var defId = currentSerial.PartDefinitionId;

                                    // Prefer to fetch existing if present
                                    var exists = await _serializablePartService.ExistsAsync(defId, serialStr);
                                    if (exists)
                                    {
                                        var existing = await _serializablePartService.GetBySerialNumberAsync(serialStr);
                                        if (existing != null)
                                            installedPart = existing;
                                        else
                                            Log.Warning("ExistsAsync returned true but GetBySerialNumberAsync returned null for serial {Serial}. Creating new.", serialStr);
                                    }
                                    else
                                    {
                                        var created = await _serializablePartService.CreateAsync(currentSerial.PartDefinition!, serialStr);
                                        if (created != null)
                                            installedPart = created;
                                        else
                                            Log.Warning("Failed to create SerializablePart for replacement serial {Serial}.", serialStr);
                                    }
                                }
                                else
                                {
                                    // No serial string: nothing to install
                                    Log.Debug("Replacement current serial for node {NodeId} lacks SerialNumber; skipping install.", entry.PartNodeId);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning(ex, "Error resolving/creating serializable part for replacement in node {NodeId}.", entry.PartNodeId);
                                continue;
                            }
                        }

                        // Add installed entry
                        partsToCreate.Add(BuildInstalledPart(savedLogId, installedPart));
                        continue;
                    }

                    // 4) Snapshot had a linked production log and current has a different linked or a serial => handle removal + optional install
                    if (snapshotLinked != null)
                    {
                        var snapshotLinkedId = snapshotLinked.Id;

                        // If still linked to same prior log => nothing to do
                        if (currentLinkedLog != null && currentLinkedLog.Id == snapshotLinkedId)
                        {
                            // Unchanged - nothing to do
                            continue;
                        }

                        // Otherwise the snapshot linked item was removed or replaced -> create Removed for the snapshot produced part
                        try
                        {
                            var produced = await _serializablePartService.GetProducedForProductionLogAsync(snapshotLinkedId);
                            if (produced != null)
                            {
                                partsToCreate.Add(BuildRemovedPart(savedLogId, produced));
                                Log.Information("Snapshot referenced linked log {LinkedId} for node {NodeId}; creating Removed entry.", snapshotLinkedId, entry.PartNodeId);
                            }
                            else
                            {
                                Log.Warning("Snapshot linked production log {LinkedId} could not be resolved to a produced part; skipping removal entry.", snapshotLinkedId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Error resolving produced part for snapshot linked log {LinkedId}.", snapshotLinkedId);
                        }

                        // If new current is a serial -> create Installed for it (resolve/create as needed)
                        if (currentSerial != null)
                        {
                            SerializablePart installedPart = currentSerial;
                            if (currentSerial.Id <= 0)
                            {
                                try
                                {
                                    var serialStr = currentSerial.SerialNumber!;
                                    var defId = currentSerial.PartDefinitionId;

                                    var exists = await _serializablePartService.ExistsAsync(defId, serialStr);
                                    if (exists)
                                    {
                                        var existing = await _serializablePartService.GetBySerialNumberAsync(serialStr);
                                        if (existing != null)
                                            installedPart = existing;
                                        else
                                            Log.Warning("ExistsAsync true but GetBySerialNumberAsync returned null for serial {Serial}. Creating new.", serialStr);
                                    }
                                    else
                                    {
                                        var created = await _serializablePartService.CreateAsync(currentSerial.PartDefinition!, serialStr);
                                        if (created != null)
                                            installedPart = created;
                                        else
                                            Log.Warning("Failed to create SerializablePart for serial {Serial}.", serialStr);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Warning(ex, "Error resolving/creating serializable part for node {NodeId}.", entry.PartNodeId);
                                    continue;
                                }
                            }

                            partsToCreate.Add(BuildInstalledPart(savedLogId, installedPart));
                        }

                        continue;
                    }

                    // 5) Snapshot had serial and current is still same -> do nothing
                    if (snapshotSerial != null && currentSerial != null && snapshotSerial.Id == currentSerial.Id)
                    {
                        // unchanged — do nothing
                        continue;
                    }

                    // 6) Snapshot empty but current has serial or linked — handled below in non-snapshot path
                }

                // ---------- NO SNAPSHOT (Scenario 1) or case fell through ----------
                if (!hasSnapshot)
                {
                    // If current is linked: attempt to create a Removed entry for the previously produced part
                    if (currentLinkedLog != null)
                    {
                        try
                        {
                            var produced =
                                await _serializablePartService.GetProducedForProductionLogAsync(currentLinkedLog.Id);
                            if (produced != null)
                            {
                                partsToCreate.Add(BuildRemovedPart(savedLogId, produced));
                                Log.Information(
                                    "Scenario1: created Removed entry for linked prior log {LinkedId} for node {NodeId}.",
                                    currentLinkedLog.Id, entry.PartNodeId);
                            }
                            else
                            {
                                Log.Warning(
                                    "Scenario1: could not resolve produced part for linked log {LinkedId}; skipping removal entry.",
                                    currentLinkedLog.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Scenario1: error resolving produced part for linked log {LinkedId}.",
                                currentLinkedLog.Id);
                        }
                    }

                    SerializablePart? partToInstall = null;

                    // 1) Try current serial first
                    if (currentSerial != null && !string.IsNullOrWhiteSpace(currentSerial.SerialNumber))
                    {
                        try
                        {
                            var serialStr = currentSerial.SerialNumber!;
                            var defId = currentSerial.PartDefinitionId;

                            if (currentSerial.PartDefinition == null)
                            {
                                Log.Warning("Scenario1: currentSerial.PartDefinition is null for node {NodeId}", entry.PartNodeId);
                            }
                            else
                            {
                                var exists = await _serializablePartService.ExistsAsync(defId, serialStr);
                                if (exists)
                                {
                                    var existing = await _serializablePartService.GetBySerialNumberAsync(serialStr);
                                    partToInstall = existing ?? await _serializablePartService.CreateAsync(currentSerial.PartDefinition, serialStr);
                                }
                                else
                                {
                                    partToInstall = await _serializablePartService.CreateAsync(currentSerial.PartDefinition, serialStr);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Scenario1: error resolving/creating serial for node {NodeId}", entry.PartNodeId);
                        }
                    }

                    // 2) If no serial, try linked production log
                    if (partToInstall == null && currentLinkedLog != null)
                    {
                        partToInstall = await _serializablePartService.GetProducedForProductionLogAsync(currentLinkedLog.Id);
                    }

                    // 3) If we found something, add Installed PLP
                    if (partToInstall != null)
                    {
                        partsToCreate.Add(BuildInstalledPart(savedLogId, partToInstall));
                        Log.Information("Scenario1: installed part for node {NodeId} using {Source}",
                            entry.PartNodeId,
                            partToInstall.Id == currentSerial?.Id ? "current serial" : "linked log");
                    }
                    else
                    {
                        Log.Warning("Scenario1: cannot create Installed PLP for node {NodeId}: no serial or linked produced part found.", entry.PartNodeId);
                    }
                    
                }
                else
                {
                    // We are in snapshot mode but some edge fell through above (e.g., snapshot missing but hasSnapshot true)
                    // Handle the common cases: if current has a serial and snapshot either didn't have it or it was different (already mostly covered),
                    // ensure we install the current serial (resolve/create) when needed.

                    // If current has a serial and it wasn't matched above, and wasn't the same as snapshot → install it
                    if (currentSerial != null)
                    {
                        // If it already has an Id (persisted), just create Installed entry
                        if (currentSerial.Id > 0)
                        {
                            partsToCreate.Add(BuildInstalledPart(savedLogId, currentSerial));
                            Log.Information("Snapshot-mode fallback: adding Installed for existing serial id {Id} for node {NodeId}.", currentSerial.Id, entry.PartNodeId);
                        }
                        else
                        {
                            try
                            {
                                // ---- REQUIRED NULLABILITY GUARDS ----
                                if (string.IsNullOrWhiteSpace(currentSerial.SerialNumber))
                                {
                                    Log.Warning(
                                        "Snapshot-mode fallback: cannot resolve/create SerializablePart for node {NodeId} because SerialNumber is null or empty.",
                                        entry.PartNodeId);
                                    continue;
                                }

                                if (currentSerial.PartDefinition == null)
                                {
                                    Log.Warning(
                                        "Snapshot-mode fallback: cannot resolve/create SerializablePart for node {NodeId} because PartDefinition is null.",
                                        entry.PartNodeId);
                                    continue;
                                }
                                // -------------------------------------

                                var serialStr = currentSerial.SerialNumber;
                                var defId = currentSerial.PartDefinitionId;

                                SerializablePart? installedPart = null;

                                // Try to reuse existing
                                var exists = await _serializablePartService.ExistsAsync(defId, serialStr);
                                if (exists)
                                {
                                    var existing = await _serializablePartService.GetBySerialNumberAsync(serialStr);
                                    if (existing != null)
                                    {
                                        installedPart = existing;
                                    }
                                    else
                                    {
                                        Log.Warning(
                                            "Snapshot fallback: ExistsAsync true but GetBySerialNumberAsync returned null for serial {Serial}. Attempting create.",
                                            serialStr);

                                        installedPart = await _serializablePartService.CreateAsync(
                                            currentSerial.PartDefinition,
                                            serialStr);
                                    }
                                }
                                else
                                {
                                    // Create new serializable part
                                    installedPart = await _serializablePartService.CreateAsync(
                                        currentSerial.PartDefinition,
                                        serialStr);
                                }

                                if (installedPart != null)
                                {
                                    partsToCreate.Add(BuildInstalledPart(savedLogId, installedPart));
                                    Log.Information(
                                        "Snapshot-mode fallback: created or used serial {Serial} and added Installed for node {NodeId}.",
                                        serialStr,
                                        entry.PartNodeId);
                                }
                                else
                                {
                                    Log.Warning(
                                        "Snapshot-mode fallback: failed to create/resolve serializable part for node {NodeId}.",
                                        entry.PartNodeId);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning(ex,
                                    "Snapshot-mode fallback: exception while resolving/creating serializable part for node {NodeId}.",
                                    entry.PartNodeId);
                            }
                        }
                    }
                }

                continue;
                
                // Helper local functions for creating ProductionLogPart objects
                ProductionLogPart BuildRemovedPart(int productionLogIdForThisSavedLog, SerializablePart removedPart)
                {
                    return new ProductionLogPart
                    {
                        ProductionLogId = productionLogIdForThisSavedLog,
                        SerializablePartId = removedPart.Id,
                        SerializablePart = removedPart,
                        OperationType = PartOperationType.Removed
                    };
                }
                
                ProductionLogPart BuildInstalledPart(int productionLogIdForThisSavedLog, SerializablePart installedPart)
                {
                    return new ProductionLogPart
                    {
                        ProductionLogId = productionLogIdForThisSavedLog,
                        SerializablePartId = installedPart.Id,
                        SerializablePart = installedPart,
                        OperationType = PartOperationType.Installed
                    };
                }
            } // foreach entry
            
            // Requirement: scenario 2 must re-produce the same serializable part
            if (snapshotGroup?.ProducedPart != null)
            {
                // Force the current produced part to be the snapshot’s produced part
                group.ProducedPart = snapshotGroup.ProducedPart;
            }
            
            // Handle ProducedPart for this log
            if (group.ProducedPart != null)
            {
                var produced = group.ProducedPart;

                SerializablePart persisted;

                // Create or get serializable part
                if (produced.Id > 0)
                {
                    persisted = produced;
                }
                else
                {
                    if (produced.PartDefinition == null)
                    {
                        Log.Warning("Produced part for log {LogIndex} has no PartDefinition; skipping.", logIndex);
                        continue;
                    }

                    persisted = await _serializablePartService.CreateAsync(
                        produced.PartDefinition, 
                        produced.SerialNumber
                    ) ?? throw new Exception("Failed to create produced part.");
                }

                // Create Installed PLP entry for this produced part
                partsToCreate.Add(new ProductionLogPart
                {
                    ProductionLogId = savedLogId,
                    SerializablePartId = persisted.Id,
                    OperationType = PartOperationType.Produced,
                });

                Log.Information("Persisted produced part for log {LogIndex}, serial {Serial}.",
                    logIndex, persisted.SerialNumber);
            }
        } // foreach group

        if (partsToCreate.Count == 0)
        {
            Log.Information("No ProductionLogPart records to persist after comparison with snapshot/current state.");
            return true;
        }

        try
        {
            var created = await _productionLogPartService.CreateRangeAsync(partsToCreate);
            if (!created)
            {
                Log.Warning("ProductionLogPartService.CreateRangeAsync returned false.");
                return false;
            }

            Log.Information("Persisted {Count} ProductionLogPart records.", partsToCreate.Count);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while persisting ProductionLogPart records.");
            return false;
        }
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