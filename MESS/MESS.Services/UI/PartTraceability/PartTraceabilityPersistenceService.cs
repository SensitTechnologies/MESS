using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.Form;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.UI.PartTraceability;

/// <inheritdoc />
public class PartTraceabilityPersistenceService : IPartTraceabilityPersistenceService
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    /// <summary>
    /// Constructs a new instance of <see cref="PartTraceabilityPersistenceService"/> with the provided database context factory.
    /// </summary>
    /// <param name="dbContextFactory"></param>
    public PartTraceabilityPersistenceService(IDbContextFactory<ApplicationContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    /// <inheritdoc />
    public List<PartTraceabilityOperation> BuildOperations(
        IEnumerable<PartTraceabilitySnapshot> snapshots,
        IReadOnlyDictionary<int, int> logIndexToProductionLogId)
    {
        ArgumentNullException.ThrowIfNull(snapshots);
        ArgumentNullException.ThrowIfNull(logIndexToProductionLogId);

        var operations = new List<PartTraceabilityOperation>();

        foreach (var snapshot in snapshots)
        {
            if (!logIndexToProductionLogId.TryGetValue(snapshot.LogIndex, out var productionLogId))
            {
                throw new InvalidOperationException(
                    $"No ProductionLogId mapping found for log index {snapshot.LogIndex}.");
            }

            var operation = new PartTraceabilityOperation
            {
                ProductionLogId = productionLogId,
                ProducedPartSerialNumber = snapshot.ProducedPartSerialNumber,
                ShouldProducePart = snapshot.ShouldProducePart,
                Entries = snapshot.Entries
                    .Select(e => new PartTraceabilityOperation.PartEntryDTO
                    {
                        PartNodeId = e.PartNodeId,
                        SerialNumber = e.SerialNumber,
                        TagCode = e.TagCode,
                        SerializablePartId = e.SerializablePartId
                    })
                    .ToList()
            };

            operations.Add(operation);
        }

        return operations;
    }
    
    ///<inheritdoc/>
    public async Task PersistOperationBatchedAsync(PartTraceabilityOperation operation)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
        
            // --- Validation ---
            if (!operation.ShouldProducePart &&
                !string.IsNullOrWhiteSpace(operation.ProducedPartSerialNumber))
            {
                throw new InvalidOperationException(
                    "ProducedPartSerialNumber was provided but ShouldProducePart is false.");
            }

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await using var tx = await db.Database.BeginTransactionAsync();

            // --- 1. Fetch production log + work instruction ---
            var productionLog = await db.ProductionLogs
                .Include(pl => pl.WorkInstruction)
                .FirstOrDefaultAsync(pl => pl.Id == operation.ProductionLogId);

            if (productionLog == null)
                throw new InvalidOperationException($"ProductionLog {operation.ProductionLogId} not found.");

            var workInstruction = productionLog.WorkInstruction;

            // --- 2. Produced part (only if WorkInstruction.PartProducedId exists) ---
            SerializablePart? producedPart = null;
            if (operation.ShouldProducePart)
            {
                if (!string.IsNullOrWhiteSpace(operation.ProducedPartSerialNumber)
                    && workInstruction?.PartProducedId != null)
                {
                    producedPart = await db.SerializableParts
                        .FirstOrDefaultAsync(p =>
                            p.SerialNumber == operation.ProducedPartSerialNumber &&
                            p.PartDefinitionId == workInstruction.PartProducedId.Value);

                    if (producedPart == null)
                    {
                        producedPart = new SerializablePart
                        {
                            SerialNumber = operation.ProducedPartSerialNumber,
                            PartDefinitionId = workInstruction.PartProducedId.Value
                        };
                        db.SerializableParts.Add(producedPart);
                    }

                    db.ProductionLogParts.Add(new ProductionLogPart
                    {
                        ProductionLogId = operation.ProductionLogId,
                        SerializablePart = producedPart,
                        OperationType = PartOperationType.Produced
                    });
                }
            }

            // --- 3. Batch fetch PartNodes ---
            var partNodeIds = operation.Entries.Select(e => e.PartNodeId).Distinct().ToList();
            var partNodes = await db.PartNodes
                .Where(n => partNodeIds.Contains(n.Id))
                .Select(n => new { n.Id, n.PartDefinitionId })
                .ToListAsync();
            var nodeToDefId = partNodes.ToDictionary(n => n.Id, n => n.PartDefinitionId);

            // --- 4. Batch fetch existing SerializableParts by ID ---
            var idsToLookup = operation.Entries
                .Where(e => e.SerializablePartId.HasValue)
                .Select(e => e.SerializablePartId!.Value)
                .Distinct()
                .ToList();

            var existingPartsById = await db.SerializableParts
                .Where(p => idsToLookup.Contains(p.Id))
                .ToListAsync();

            var idToPart = existingPartsById.ToDictionary(p => p.Id, p => p);

            // --- 5. Batch fetch Tags ---
            var tagCodes = operation.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.TagCode))
                .Select(e => e.TagCode!)
                .Distinct()
                .ToList();

            var tags = await db.Tags
                .Include(t => t.SerializablePart)
                .Where(t => tagCodes.Contains(t.Code))
                .ToListAsync();

            var codeToTag = tags.ToDictionary(t => t.Code, t => t);

            // --- 6. Prepare parts to add and map entries ---
            var partsToAdd = new List<SerializablePart>();
            var entryPartMap = new Dictionary<PartTraceabilityOperation.PartEntryDTO, SerializablePart>();

            foreach (var entry in operation.Entries)
            {
                SerializablePart? part;

                if (entry.SerializablePartId.HasValue)
                {
                    if (!idToPart.TryGetValue(entry.SerializablePartId.Value, out part))
                        throw new InvalidOperationException(
                            $"SerializablePartId '{entry.SerializablePartId}' not found.");

                    if (part.PartDefinitionId != nodeToDefId[entry.PartNodeId])
                        throw new InvalidOperationException(
                            $"SerializablePartId '{entry.SerializablePartId}' has PartDefinitionId {part.PartDefinitionId} " +
                            $"but PartNode {entry.PartNodeId} expects {nodeToDefId[entry.PartNodeId]}.");
                }
                else if (!string.IsNullOrWhiteSpace(entry.TagCode))
                {
                    if (!codeToTag.TryGetValue(entry.TagCode!, out var tag))
                        throw new InvalidOperationException($"Tag '{entry.TagCode}' not found.");

                    // Safety checks
                    if (tag.Status == TagStatus.Retired)
                        throw new InvalidOperationException($"Tag '{entry.TagCode}' is retired and cannot be used.");

                    if (tag.Status != TagStatus.Assigned || tag.SerializablePart == null)
                        throw new InvalidOperationException($"Tag '{entry.TagCode}' is not currently assigned to a part.");

                    part = tag.SerializablePart;

                    // Validate PartDefinition
                    if (part.PartDefinitionId != nodeToDefId[entry.PartNodeId])
                        throw new InvalidOperationException(
                            $"Resolved part for Tag '{entry.TagCode}' has PartDefinitionId {part.PartDefinitionId} " +
                            $"but PartNode {entry.PartNodeId} expects {nodeToDefId[entry.PartNodeId]}.");

                    // Optional: prevent duplicate usage in same operation
                    if (entryPartMap.Values.Any(p => p.Id == part.Id))
                        throw new InvalidOperationException($"Tag '{entry.TagCode}' is assigned to a part already used in this operation.");

                    // --- TagHistory creation ---
                    tag.History.Add(new TagHistory
                    {
                        TagId = tag.Id,
                        SerializablePartId = part.Id,
                        EventType = TagEventType.Assigned,
                        Timestamp = DateTimeOffset.UtcNow
                    });
                }
                else if (!string.IsNullOrWhiteSpace(entry.SerialNumber))
                {
                    // Always create new part for serial number
                    part = new SerializablePart
                    {
                        SerialNumber = entry.SerialNumber,
                        PartDefinitionId = nodeToDefId[entry.PartNodeId]
                    };
                    partsToAdd.Add(part);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot resolve part for PartNodeId {entry.PartNodeId}");
                }

                entryPartMap[entry] = part;
            }

            db.SerializableParts.AddRange(partsToAdd);

            // --- 7. ProductionLogParts and relationships ---
            foreach (var kvp in entryPartMap)
            {
                var entry = kvp.Key;
                var part = kvp.Value;

                // ProductionLogPart
                db.ProductionLogParts.Add(new ProductionLogPart
                {
                    ProductionLogId = operation.ProductionLogId,
                    SerializablePart = part,
                    OperationType = PartOperationType.Installed
                });

                // Link installed parts to produced part
                if (producedPart != null)
                {
                    db.SerializablePartRelationships.Add(new SerializablePartRelationship
                    {
                        ParentPart = producedPart,
                        ChildPart = part,
                        LastUpdated = DateTimeOffset.UtcNow
                    });
                }
            }

            // --- 8. Save everything in one batch ---
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        });
    }
}
