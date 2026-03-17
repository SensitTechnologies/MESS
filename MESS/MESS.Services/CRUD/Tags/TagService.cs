using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Services.DTOs.Tags;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.Tags;

/// <summary>
/// Provides database-backed implementation of <see cref="ITagService"/>.
/// </summary>
public class TagService : ITagService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagService"/> class.
    /// </summary>
    /// <param name="contextFactory">Factory used to create database contexts.</param>
    public TagService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ---------------------------
    // Creation
    // ---------------------------

    /// <inheritdoc />
    public async Task<TagDTO> CreateAsync(TagCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Tag code must be provided.", nameof(request));

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if the tag already exists
        var existingTag = await context.Tags
            .FirstOrDefaultAsync(t => t.Code == request.Code);

        if (existingTag != null)
            throw new InvalidOperationException($"A tag with code '{request.Code}' already exists.");

        var now = DateTimeOffset.UtcNow;

        // Create the tag entity
        var tag = new Tag
        {
            Code = request.Code,
            Status = TagStatus.Available,
            CreatedAt = now
        };

        await context.Tags.AddAsync(tag);
        await context.SaveChangesAsync();

        // Record creation in tag history
        var history = new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Created,
            Timestamp = now
        };

        await context.TagHistories.AddAsync(history);
        await context.SaveChangesAsync();

        // Return a fully populated DTO
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId,
            PartSerialNumber = tag.SerializablePart?.SerialNumber,
            PartName = tag.SerializablePart?.PartDefinition?.Name
            // WorkInstructionTitle could be set if desired from production logs
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagDTO>> BulkCreateAsync(TagBatchCreateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Count <= 0)
            throw new ArgumentException("Count must be greater than zero.", nameof(request));

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Generate codes using your generator
        var codes = TagCodeGenerator.Generate(request).ToList();

        if (codes.Count != request.Count)
            throw new InvalidOperationException("Generated code count does not match requested count.");

        // Check for duplicates within the batch itself
        var duplicateCodes = codes
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCodes.Count != 0)
            throw new InvalidOperationException("Duplicate tag codes were generated in the batch.");

        // Check for existing codes in database
        var existingCodes = await context.Tags
            .Where(t => codes.Contains(t.Code))
            .Select(t => t.Code)
            .ToListAsync();

        if (existingCodes.Any())
            throw new InvalidOperationException(
                $"The following tag codes already exist: {string.Join(", ", existingCodes)}");

        var now = DateTimeOffset.UtcNow;

        // Create tag entities with history
        var tags = codes.Select(code => new Tag
        {
            Code = code,
            Status = TagStatus.Available,
            CreatedAt = now,

            History = new List<TagHistory>
            {
                new TagHistory
                {
                    EventType = TagEventType.Created,
                    Timestamp = now
                }
            }
        }).ToList();

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        // Map to DTOs (light mapping only — no navigation assumptions)
        return tags.Select(t => new TagDTO
        {
            Id = t.Id,
            Code = t.Code,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
            SerializablePartId = t.SerializablePartId
        }).ToList();
    }


    // ---------------------------
    // Assignment
    // ---------------------------

    /// <inheritdoc />
    public async Task<TagDTO> AssignAsync(int tagId, int serializablePartId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load tag
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} was not found.");

        // Validate state
        if (tag.Status == TagStatus.Retired)
            throw new InvalidOperationException("Cannot assign a retired tag.");

        if (tag.Status == TagStatus.Assigned)
            throw new InvalidOperationException("Tag is already assigned.");

        // Ensure the serializable part exists
        var partExists = await context.SerializableParts
            .AnyAsync(sp => sp.Id == serializablePartId);

        if (!partExists)
            throw new KeyNotFoundException($"SerializablePart with ID {serializablePartId} was not found.");

        var now = DateTimeOffset.UtcNow;

        // Update tag state
        tag.Status = TagStatus.Assigned;
        tag.SerializablePartId = serializablePartId;

        // Add history event
        context.TagHistories.Add(new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Assigned,
            Timestamp = now
        });

        await context.SaveChangesAsync();

        // Return DTO (light mapping)
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId
        };
    }

    /// <inheritdoc />
    public async Task<TagDTO> UnassignAsync(int tagId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load tag
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} was not found.");

        // Validate state
        if (tag.Status == TagStatus.Retired)
            throw new InvalidOperationException("Cannot unassign a retired tag.");

        if (tag.Status != TagStatus.Assigned || tag.SerializablePartId == null)
            throw new InvalidOperationException("Tag is not currently assigned.");

        var now = DateTimeOffset.UtcNow;

        // Update tag state
        tag.Status = TagStatus.Available;
        tag.SerializablePartId = null;

        // Record history
        context.TagHistories.Add(new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Unassigned,
            Timestamp = now
        });

        await context.SaveChangesAsync();

        // Return DTO (light mapping)
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId
        };
    }

    /// <inheritdoc />
    public async Task<TagDTO> RetireAsync(int tagId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load tag
        var tag = await context.Tags
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} was not found.");

        // Validate state
        if (tag.Status == TagStatus.Retired)
            throw new InvalidOperationException("Tag is already retired.");

        var now = DateTimeOffset.UtcNow;

        // If currently assigned, unassign as part of retirement
        if (tag is { Status: TagStatus.Assigned, SerializablePartId: not null })
        {
            tag.SerializablePartId = null;

            context.TagHistories.Add(new TagHistory
            {
                TagId = tag.Id,
                EventType = TagEventType.Unassigned,
                Timestamp = now
            });
        }

        // Update state to retired
        tag.Status = TagStatus.Retired;

        // Record retirement history
        context.TagHistories.Add(new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Retired,
            Timestamp = now
        });

        await context.SaveChangesAsync();

        // Return DTO (light mapping)
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId
        };
    }

    /// <inheritdoc />
    public async Task<TagDTO> MarkPrintedAsync(int tagId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load the tag with its related SerializablePart and PartDefinition
        var tag = await context.Tags
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition) // null-forgiving to satisfy EF compiler
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} was not found.");

        // Optionally prevent printing retired tags
        // if (tag.Status == TagStatus.Retired)
        //     throw new InvalidOperationException("Cannot print a retired tag.");

        var now = DateTimeOffset.UtcNow;

        // Record the print event in tag history
        var history = new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Printed,
            Timestamp = now,
            SerializablePartId = tag.SerializablePartId // optional, only if assigned
        };

        context.TagHistories.Add(history);

        // Save changes
        await context.SaveChangesAsync();

        // Return DTO
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId,
            PartSerialNumber = tag.SerializablePart?.SerialNumber,
            PartName = tag.SerializablePart?.PartDefinition?.Name
            // WorkInstructionTitle can be populated if you load related production log data
        };
    }


    // ---------------------------
    // Lookup
    // ---------------------------

    /// <inheritdoc />
    public async Task<TagDTO?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load the tag with related SerializablePart and PartDefinition
        var tag = await context.Tags
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition) // null-forgiving for EF
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.ProductionLogParts)
            .ThenInclude(plp => plp.ProductionLog)
            .ThenInclude(pl => pl!.WorkInstruction)
            .FirstOrDefaultAsync(t => t.Code == code);

        if (tag == null)
            return null;

        // Determine the most recent work instruction title
        string? workInstructionTitle = null;
        if (tag.SerializablePart?.ProductionLogParts != null)
        {
            workInstructionTitle = tag.SerializablePart.ProductionLogParts
                .Where(plp => plp.ProductionLog?.WorkInstruction != null)
                .OrderByDescending(plp => plp.ProductionLog!.CreatedOn) // most recent
                .Select(plp => plp.ProductionLog!.WorkInstruction!.Title)
                .FirstOrDefault();
        }

        // Map to DTO
        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId,
            PartSerialNumber = tag.SerializablePart?.SerialNumber,
            PartName = tag.SerializablePart?.PartDefinition?.Name,
            WorkInstructionTitle = workInstructionTitle
        };
    }

    /// <inheritdoc />
    public async Task<TagDTO?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load the tag along with its related data
        var tag = await context.Tags
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.ProductionLogParts)
            .ThenInclude(plp => plp.ProductionLog!)
            .ThenInclude(pl => pl.WorkInstruction)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
            return null;

        // Optionally, get the last work instruction title from the production logs
        var lastWorkInstructionTitle = tag.SerializablePart?
            .ProductionLogParts
            .OrderByDescending(plp => plp.ProductionLog!.CreatedOn)
            .Select(plp => plp.ProductionLog!.WorkInstruction?.Title)
            .FirstOrDefault();

        return new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId,
            PartSerialNumber = tag.SerializablePart?.SerialNumber,
            PartName = tag.SerializablePart?.PartDefinition?.Name,
            WorkInstructionTitle = lastWorkInstructionTitle
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagDTO>> GetBySerializablePartAsync(int serializablePartId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load tags with related data for the given serializable part
        var tags = await context.Tags
            .Where(t => t.SerializablePartId == serializablePartId)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.ProductionLogParts)
            .ThenInclude(plp => plp.ProductionLog!)
            .ThenInclude(pl => pl.WorkInstruction)
            .ToListAsync();

        // Map to DTOs
        var tagDtos = tags.Select(tag =>
        {
            // Determine the most recent work instruction title for this tag
            var lastWorkInstructionTitle = tag.SerializablePart?
                .ProductionLogParts
                .OrderByDescending(plp => plp.ProductionLog!.CreatedOn)
                .Select(plp => plp.ProductionLog!.WorkInstruction?.Title)
                .FirstOrDefault();

            return new TagDTO
            {
                Id = tag.Id,
                Code = tag.Code,
                Status = tag.Status,
                CreatedAt = tag.CreatedAt,
                SerializablePartId = tag.SerializablePartId,
                PartSerialNumber = tag.SerializablePart?.SerialNumber,
                PartName = tag.SerializablePart?.PartDefinition?.Name,
                WorkInstructionTitle = lastWorkInstructionTitle
            };
        }).ToList();

        return tagDtos;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagDTO>> GetAvailableAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Retrieve all tags that are available (not assigned, not retired)
        var tags = await context.Tags
            .Where(t => t.Status == TagStatus.Available)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.ProductionLogParts)
            .ThenInclude(plp => plp.ProductionLog!)
            .ThenInclude(pl => pl.WorkInstruction)
            .ToListAsync();

        // Map to DTOs
        var tagDtos = tags.Select(tag =>
        {
            var lastWorkInstructionTitle = tag.SerializablePart?
                .ProductionLogParts
                .OrderByDescending(plp => plp.ProductionLog!.CreatedOn)
                .Select(plp => plp.ProductionLog!.WorkInstruction?.Title)
                .FirstOrDefault();

            return new TagDTO
            {
                Id = tag.Id,
                Code = tag.Code,
                Status = tag.Status,
                CreatedAt = tag.CreatedAt,
                SerializablePartId = tag.SerializablePartId,
                PartSerialNumber = tag.SerializablePart?.SerialNumber,
                PartName = tag.SerializablePart?.PartDefinition?.Name,
                WorkInstructionTitle = lastWorkInstructionTitle
            };
        }).ToList();

        return tagDtos;
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tag code cannot be null or empty.", nameof(code));

        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check for existence of a tag with the given code that is available
        return await context.Tags
            .AnyAsync(t => t.Code == code && t.Status == TagStatus.Available);
    }


    // ---------------------------
    // History
    // ---------------------------

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagHistory>> GetHistoryAsync(int tagId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load history for the specified tag, ordered by timestamp
        var history = await context.TagHistories
            .Where(h => h.TagId == tagId)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();

        return history;
    }
}