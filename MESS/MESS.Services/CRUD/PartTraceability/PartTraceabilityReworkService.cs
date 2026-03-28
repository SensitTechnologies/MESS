using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.UI.PartTraceability;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.PartTraceability;

///<inheritdoc/>
public class PartTraceabilityReworkService : IPartTraceabilityReworkService
{
    private readonly IDbContextFactory<ApplicationContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartTraceabilityReworkService"/> class.
    /// </summary>
    /// <param name="dbContextFactory">
    /// A factory used to create <see cref="ApplicationContext"/> instances for database access.
    /// This ensures safe, short-lived DbContext usage within the service.
    /// </param>
    public PartTraceabilityReworkService(
        IDbContextFactory<ApplicationContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    ///<inheritdoc/>
    public async Task<List<PartTraceabilitySnapshot>> BuildSnapshotsFromTagCodesAsync(
        List<string> tagCodes,
        int workInstructionId)
    {
        if (tagCodes.Count == 0)
        {
            Log.Information("No tag codes provided for snapshot build.");
            return [];
        }

        Log.Information("Building part traceability snapshots for {TagCount} tags using WorkInstruction {WorkInstructionId}",
            tagCodes.Count, workInstructionId);

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        // --- Load Work Instruction nodes ---
        var workInstruction = await db.WorkInstructions
            .Include(wi => wi.Nodes)
            .FirstOrDefaultAsync(wi => wi.Id == workInstructionId);

        if (workInstruction == null)
        {
            Log.Error("Work instruction {WorkInstructionId} not found.", workInstructionId);
            throw new InvalidOperationException("Work instruction not found.");
        }

        var partNodes = workInstruction.Nodes
            .Where(n => n.NodeType == WorkInstructionNodeType.Part)
            .Cast<PartNode>()
            .ToList();

        Log.Debug("Loaded {PartNodeCount} part nodes for WorkInstruction {WorkInstructionId}",
            partNodes.Count, workInstructionId);

        var partNodeMap = partNodes
            .ToDictionary(n => n.PartDefinitionId, n => n.Id);

        // --- Load tags (roots) ---
        var tags = await db.Tags
            .Where(t => tagCodes.Contains(t.Code))
            .Include(t => t.SerializablePart)
            .ToListAsync();

        Log.Information("Resolved {ResolvedTagCount}/{RequestedTagCount} tags from database.",
            tags.Count, tagCodes.Count);

        // --- Load relationships ---
        var relationships = await db.SerializablePartRelationships
            .Include(r => r.ChildPart)
            .ToListAsync();

        var childrenLookup = relationships
            .Where(r => r.ParentPartId != null)
            .GroupBy(r => r.ParentPartId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // --- Load tag lookup for all parts ---
        var partIds = relationships
            .Select(r => r.ChildPartId)
            .Concat(relationships.Where(r => r.ParentPartId != null).Select(r => r.ParentPartId!.Value))
            .Distinct()
            .ToList();

        var tagLookup = await db.Tags
            .Where(t => t.SerializablePartId != null && partIds.Contains(t.SerializablePartId.Value))
            .ToDictionaryAsync(t => t.SerializablePartId!.Value, t => t);

        var snapshots = new List<PartTraceabilitySnapshot>();

        int logIndex = 0;

        foreach (var tag in tags)
        {
            if (tag.SerializablePart == null)
            {
                Log.Warning("Tag {TagCode} has no associated SerializablePart. Skipping.", tag.Code);
                continue;
            }

            var root = tag.SerializablePart;

            Log.Debug("Processing tag {TagCode} for SerializablePart {PartId}", tag.Code, root.Id);

            // --- Traverse tree ---
            var collectedParts = new List<SerializablePart>();

            void Traverse(SerializablePart part)
            {
                collectedParts.Add(part);

                if (childrenLookup.TryGetValue(part.Id, out var children) && children is { })
                {
                    foreach (var rel in children)
                    {
                        if (rel.ChildPart != null)
                            Traverse(rel.ChildPart);
                    }
                }
            }

            Traverse(root);

            Log.Debug("Collected {PartCount} parts in tree for root {RootPartId}", 
                collectedParts.Count, root.Id);

            // --- Build snapshot ---
            var snapshot = new PartTraceabilitySnapshot
            {
                LogIndex = logIndex++,
                ProducedPartSerialNumber = root.SerialNumber,
                ProducedPartTagCode = tag.Code,
                ShouldProducePart = false,
                Entries = new List<PartTraceabilitySnapshot.PartEntrySnapshot>()
            };

            foreach (var part in collectedParts)
            {
                if (part.Id == root.Id)
                    continue;

                if (!partNodeMap.TryGetValue(part.PartDefinitionId, out var partNodeId))
                    continue;

                tagLookup.TryGetValue(part.Id, out var childTag);

                snapshot.Entries.Add(new PartTraceabilitySnapshot.PartEntrySnapshot
                {
                    PartNodeId = partNodeId,
                    SerialNumber = part.SerialNumber,
                    TagCode = childTag?.Code,
                    SerializablePartId = part.Id
                });
            }

            Log.Debug("Snapshot {LogIndex} built with {EntryCount} entries",
                snapshot.LogIndex, snapshot.Entries.Count);

            snapshots.Add(snapshot);
        }

        Log.Information("Built {SnapshotCount} part traceability snapshots.", snapshots.Count);

        return snapshots;
    }
}