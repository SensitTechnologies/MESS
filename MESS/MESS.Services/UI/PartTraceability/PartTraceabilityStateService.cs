using MESS.Services.CRUD.SerializableParts;

namespace MESS.Services.UI.PartTraceability;

/// <inheritdoc/>
public class PartTraceabilityStateService : IPartTraceabilityStateService 
{
    private readonly ISerializablePartService _serializablePartService;
    
    // Fast lookup: logIndex → (partNodeId → entry)
    private readonly Dictionary<int, Dictionary<int, PartEntryState>> _entriesByLogIndex = new();

    // Produced part serial numbers per log
    private readonly Dictionary<int, string?> _producedPartSerialNumbers = new();

    
    /// <summary>
    /// Creates a new instance of <see cref="PartTraceabilityStateService"/>.
    /// </summary>
    /// <param name="serializablePartService">
    /// The service used to resolve tag codes to serializable parts.
    /// </param>
    public PartTraceabilityStateService(ISerializablePartService serializablePartService)
    {
        _serializablePartService = serializablePartService;
    }
    
    /// <inheritdoc/>
    public void Initialize(IEnumerable<int> logIndexes, IEnumerable<List<int>> nodeBlocks)
    {
        _entriesByLogIndex.Clear();
        _producedPartSerialNumbers.Clear();

        var blocks = nodeBlocks.ToList();

        foreach (var logIndex in logIndexes)
        {
            var entries = new Dictionary<int, PartEntryState>();

            foreach (var block in blocks)
            {
                foreach (var partNodeId in block)
                {
                    if (!entries.ContainsKey(partNodeId))
                        entries[partNodeId] = new PartEntryState { PartNodeId = partNodeId };
                }
            }

            _entriesByLogIndex[logIndex] = entries;
            _producedPartSerialNumbers[logIndex] = null;
        }
    }

    /// <inheritdoc/>
    public PartEntryState GetEntry(int logIndex, int partNodeId)
    {
        if (!_entriesByLogIndex.TryGetValue(logIndex, out var entries) ||
            !entries.TryGetValue(partNodeId, out var entry))
        {
            throw new KeyNotFoundException(
                $"Entry not found for logIndex {logIndex}, partNodeId {partNodeId}");
        }

        return entry;
    }

    /// <inheritdoc/>
    public bool TryGetEntry(int logIndex, int partNodeId, out PartEntryState? entry)
    {
        entry = null;
        if (_entriesByLogIndex.TryGetValue(logIndex, out var entries))
        {
            return entries.TryGetValue(partNodeId, out entry);
        }
        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<PartEntryState> GetEntries(int logIndex)
    {
        if (!_entriesByLogIndex.TryGetValue(logIndex, out var entries))
            throw new KeyNotFoundException($"No entries found for logIndex {logIndex}");

        return entries.Values.ToList().AsReadOnly();
    }


    /// <inheritdoc/>
    public bool TryGetEntries(int logIndex, out IReadOnlyCollection<PartEntryState>? entries)
    {
        entries = null;
        if (_entriesByLogIndex.TryGetValue(logIndex, out var dict))
        {
            entries = dict.Values.ToList().AsReadOnly();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateTagCodeAsync(int logIndex, int partNodeId, string? tagCode)
    {
        var entry = GetEntry(logIndex, partNodeId);
        entry.TagCode = tagCode;
        entry.SerializablePartId = null; // clear previous resolved ID

        if (string.IsNullOrWhiteSpace(tagCode))
        {
            // No tag entered → nothing to resolve
            return false;
        }

        // Try to resolve the tag code to a serializable part ID
        var resolvedId = await _serializablePartService.TryResolveTagAsync(tagCode);

        if (resolvedId.HasValue)
        {
            entry.SerializablePartId = resolvedId.Value;
            return true; // caller knows we resolved something
        }

        return false; // tag code entered but could not be resolved
    }

    /// <inheritdoc/>
    public void UpdateSerialNumber(int logIndex, int partNodeId, string? serialNumber)
    {
        var entry = GetEntry(logIndex, partNodeId);
        entry.SerialNumber = serialNumber;
    }
    
    /// <inheritdoc/>
    public bool RemoveLog(int logIndex)
    {
        var removed = _entriesByLogIndex.Remove(logIndex);
        _producedPartSerialNumbers.Remove(logIndex);
        return removed;
    }

    /// <inheritdoc/>
    public bool HasLog(int logIndex) => _entriesByLogIndex.ContainsKey(logIndex);

    /// <inheritdoc/>
    public void Clear()
    {
        _entriesByLogIndex.Clear();
        _producedPartSerialNumbers.Clear();
    }

    /// <inheritdoc/>
    public void SetProducedPartSerialNumber(int logIndex, string? serialNumber)
    {
        if (!_entriesByLogIndex.ContainsKey(logIndex))
            throw new KeyNotFoundException($"Log index {logIndex} does not exist.");

        _producedPartSerialNumbers[logIndex] = serialNumber;
    }
    
    /// <inheritdoc/>
    public PartTraceabilitySnapshot CreateSnapshot(int logIndex)
    {
        if (!_entriesByLogIndex.TryGetValue(logIndex, out var entries))
            throw new InvalidOperationException($"No entries found for logIndex {logIndex}");

        var snapshotEntries = entries.Values
            .Select(e => new PartTraceabilitySnapshot.PartEntrySnapshot
            {
                PartNodeId = e.PartNodeId,
                SerialNumber = e.SerialNumber,
                TagCode = e.TagCode,
                SerializablePartId = e.SerializablePartId
            })
            .ToList();

        _producedPartSerialNumbers.TryGetValue(logIndex, out var producedSerial);

        return new PartTraceabilitySnapshot
        {
            LogIndex = logIndex,
            ProducedPartSerialNumber = producedSerial,
            Entries = snapshotEntries
        };
    }
}