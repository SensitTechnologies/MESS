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
    /// OperationType is currently always <see cref="PartOperationType.Installed"/>.
    /// </summary>
    public async Task<bool> PersistAsync(List<ProductionLogFormDTO> savedLogs)
    {
        if (savedLogs == null) throw new ArgumentNullException(nameof(savedLogs));

        var partsToCreate = new List<ProductionLogPart>();

        foreach (var group in _entryGroups.Values)
        {
            var logIndex = group.LogIndex;

            // If there is no corresponding saved log, skip (caller should pass matching savedLogs)
            if (logIndex < 0 || logIndex >= savedLogs.Count)
            {
                Log.Warning("No saved production log for log index {LogIndex}; skipping group.", logIndex);
                continue;
            }

            var savedLogId = savedLogs[logIndex].Id;
            if (savedLogId <= 0)
            {
                Log.Warning("Saved production log at index {LogIndex} has invalid Id {Id}; skipping group.", logIndex, savedLogId);
                continue;
            }

            // iterate entries inside this group
            foreach (var entry in group.PartNodeEntries)
            {
                SerializablePart? resolvedPart = null;

                // prefer linked production log (if present)
                if (entry.LinkedProductionLog is not null)
                {
                    try
                    {
                        var linkedLogId = entry.LinkedProductionLog.Id;
                        var produced = await _serializablePartService.GetProducedForProductionLogAsync(linkedLogId);
                        if (produced != null)
                        {
                            resolvedPart = produced;
                        }
                        else
                        {
                            Log.Warning("No produced serializable part found for linked production log id {LinkedLogId}.", linkedLogId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Error fetching produced part for linked production log id {LinkedLogId}.", entry.LinkedProductionLog.Id);
                    }
                }

                // fallback to explicit serial number entry
                if (resolvedPart == null && entry.SerializablePart is not null && !string.IsNullOrWhiteSpace(entry.SerializablePart.SerialNumber))
                {
                    var serial = entry.SerializablePart.SerialNumber!;
                    var defId = entry.SerializablePart.PartDefinitionId;

                    try
                    {
                        // if exists, fetch existing record; otherwise create a new serializable part record
                        var exists = await _serializablePartService.ExistsAsync(defId, serial);
                        if (exists)
                        {
                            var existing = await _serializablePartService.GetBySerialNumberAsync(serial);
                            if (existing != null)
                                resolvedPart = existing;
                            else
                                Log.Warning("ExistsAsync returned true but GetBySerialNumberAsync returned null for serial {Serial}.", serial);
                        }
                        else
                        {
                            // create new
                            var created = await _serializablePartService.CreateAsync(entry.SerializablePart.PartDefinition!, serial);
                            if (created != null)
                                resolvedPart = created;
                            else
                                Log.Warning("Failed to create SerializablePart for part definition {DefId} serial '{Serial}'.", defId, serial);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Error resolving/creating SerializablePart for serial '{Serial}' (PartDefinitionId {DefId}).", serial, defId);
                    }
                }

                if (resolvedPart == null)
                {
                    Log.Debug("Skipping part entry for PartNodeId {PartNodeId} in log index {LogIndex} because no serializable part was resolved.", entry.PartNodeId, logIndex);
                    continue;
                }

                // Build ProductionLogPart (Installed)
                var plp = new ProductionLogPart
                {
                    ProductionLogId = savedLogId,
                    SerializablePartId = resolvedPart.Id,
                    SerializablePart = resolvedPart,
                    OperationType = PartOperationType.Installed
                };

                partsToCreate.Add(plp);
            }
            
            // Handle produced part
            if (group.ProducedPart == null) continue;
            {
                var p = group.ProducedPart;

                // Create the produced part record exactly as provided, even if the
                // serial number is null or empty.
                var created = await _serializablePartService.CreateAsync(
                    p.PartDefinition!, p.SerialNumber);

                if (created != null)
                {
                    partsToCreate.Add(new ProductionLogPart
                    {
                        ProductionLogId = savedLogId,
                        SerializablePartId = created.Id,
                        SerializablePart = created,
                        OperationType = PartOperationType.Produced
                    });
                }
            }
        }

        if (partsToCreate.Count == 0)
        {
            Log.Information("No ProductionLogPart records to persist.");
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
}