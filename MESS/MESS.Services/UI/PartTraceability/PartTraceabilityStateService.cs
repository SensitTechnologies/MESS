using System.Text;
using MESS.Services.CRUD.SerializableParts;

namespace MESS.Services.UI.PartTraceability;

/// <inheritdoc/>
public class PartTraceabilityStateService : IPartTraceabilityStateService 
{
    private readonly ISerializablePartService _serializablePartService;
    
    private readonly Dictionary<int, LogState> _logs = new();

    
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
        _logs.Clear();

        var blocks = nodeBlocks.ToList();

        foreach (var logIndex in logIndexes)
        {
            var logState = new LogState
            {
                LogIndex = logIndex
            };

            foreach (var block in blocks)
            {
                foreach (var partNodeId in block)
                {
                    if (!logState.Entries.ContainsKey(partNodeId))
                    {
                        logState.Entries[partNodeId] = new PartEntryState
                        {
                            PartNodeId = partNodeId
                        };
                    }
                }
            }

            _logs[logIndex] = logState;
        }
    }

    /// <inheritdoc/>
    public PartEntryState GetEntry(int logIndex, int partNodeId)
    {
        if (!_logs.TryGetValue(logIndex, out var log) ||
            !log.Entries.TryGetValue(partNodeId, out var entry))
        {
            throw new KeyNotFoundException(
                $"Entry not found for logIndex {logIndex}, partNodeId {partNodeId}");
        }

        return entry;
    }
    
    /// <inheritdoc/>
    public PartEntryState AddOrGetEntry(int logIndex, int partNodeId)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
        {
            log = new LogState
            {
                LogIndex = logIndex
            };

            _logs[logIndex] = log;
        }

        if (!log.Entries.TryGetValue(partNodeId, out var entry))
        {
            entry = new PartEntryState
            {
                PartNodeId = partNodeId
            };

            log.Entries[partNodeId] = entry;
        }

        return entry;
    }

    /// <inheritdoc/>
    public bool TryGetEntry(int logIndex, int partNodeId, out PartEntryState? entry)
    {
        entry = null;

        return _logs.TryGetValue(logIndex, out var log) && log.Entries.TryGetValue(partNodeId, out entry);
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<PartEntryState> GetEntries(int logIndex)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
            throw new KeyNotFoundException($"No entries found for logIndex {logIndex}");

        return log.Entries.Values.ToList().AsReadOnly();
    }


    /// <inheritdoc/>
    public bool TryGetEntries(int logIndex, out IReadOnlyCollection<PartEntryState>? entries)
    {
        entries = null;

        if (_logs.TryGetValue(logIndex, out var log))
        {
            entries = log.Entries.Values.ToList().AsReadOnly();
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
    public void SetShouldProducePart(int logIndex, bool shouldProduce)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
            throw new KeyNotFoundException($"Log index {logIndex} does not exist.");

        log.ShouldProducePart = shouldProduce;
    }
    
    /// <inheritdoc/>
    public bool ShouldCreateProducedPart(int logIndex)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
            throw new KeyNotFoundException($"Log index {logIndex} does not exist.");

        return log.ShouldProducePart;
    }
    
    /// <inheritdoc/>
    public bool RemoveLog(int logIndex)
    {
        return _logs.Remove(logIndex);
    }

    /// <inheritdoc/>
    public bool HasLog(int logIndex) => _logs.ContainsKey(logIndex);
    
    /// <inheritdoc/>
    public int GetTotalPartsLogged()
    {
        return _logs.Values
            .SelectMany(log => log.Entries.Values)
            .Count(entry => entry.HasInput);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _logs.Clear();
    }

    /// <inheritdoc/>
    public void SetProducedPartSerialNumber(int logIndex, string? serialNumber)
    {
        var log = EnsureLogExists(logIndex);
        log.ProducedPartSerialNumber = serialNumber;
    }
    
    /// <summary>
    /// Ensures that a <see cref="LogState"/> exists for the given log index.
    /// If it does not exist, it is created.
    /// </summary>
    /// <param name="logIndex">The log index to ensure.</param>
    /// <returns>The existing or newly created <see cref="LogState"/>.</returns>
    private LogState EnsureLogExists(int logIndex)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
        {
            log = new LogState
            {
                LogIndex = logIndex
            };

            _logs[logIndex] = log;
        }

        return log;
    }
    
    /// <inheritdoc/>
    public PartTraceabilitySnapshot CreateSnapshot(int logIndex)
    {
        if (!_logs.TryGetValue(logIndex, out var log))
            throw new InvalidOperationException($"No entries found for logIndex {logIndex}");

        var snapshotEntries = log.Entries.Values
            .Select(e => new PartTraceabilitySnapshot.PartEntrySnapshot
            {
                PartNodeId = e.PartNodeId,
                SerialNumber = e.SerialNumber,
                TagCode = e.TagCode,
                SerializablePartId = e.SerializablePartId
            })
            .ToList();

        return new PartTraceabilitySnapshot
        {
            LogIndex = logIndex,
            ProducedPartSerialNumber = log.ProducedPartSerialNumber,
            ShouldProducePart = log.ShouldProducePart, 
            Entries = snapshotEntries
        };
    }
    
    /// <inheritdoc/>
    public string Dump(int? logIndexFilter = null, bool onlyWithInput = false)
    {
        var sb = new StringBuilder();

        sb.AppendLine("===== PartTraceabilityStateService Dump =====");

        if (_logs.Count == 0)
        {
            sb.AppendLine("No logs present.");
            return sb.ToString();
        }

        foreach (var (logIndex, log) in _logs)
        {
            if (logIndexFilter.HasValue && logIndex != logIndexFilter.Value)
                continue;

            sb.AppendLine($"\nLogIndex: {logIndex}");
            sb.AppendLine($"  ShouldProducePart: {log.ShouldProducePart}");
            sb.AppendLine($"  ProducedPartSerialNumber: {log.ProducedPartSerialNumber ?? "[null]"}");

            if (log.Entries.Count == 0)
            {
                sb.AppendLine("  No entries.");
                continue;
            }

            foreach (var (partNodeId, entry) in log.Entries)
            {
                if (onlyWithInput && !entry.HasInput)
                    continue;

                var marker = entry.HasInput ? "[X]" : "[ ]";

                sb.AppendLine($"  {marker} PartNodeId: {partNodeId}");
                sb.AppendLine($"    SerialNumber: {entry.SerialNumber ?? "[null]"}");
                sb.AppendLine($"    TagCode: {entry.TagCode ?? "[null]"}");
                sb.AppendLine($"    SerializablePartId: {entry.SerializablePartId?.ToString() ?? "[null]"}");
            }
        }

        sb.AppendLine("\n===== End Dump =====");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public void DumpToConsole(int? logIndexFilter = null, bool onlyWithInput = false)
    {
        Console.WriteLine(Dump(logIndexFilter, onlyWithInput));
    }
}