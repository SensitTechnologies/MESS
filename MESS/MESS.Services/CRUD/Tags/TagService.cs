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
        
        // Get existing codes from DB
        var existingCodes = await context.Tags
            .Select(t => t.Code)
            .ToListAsync();

        // Determine starting index
        var startIndex = DetectNextIndex(existingCodes, request.Scheme, request.Prefix);

        // Generate codes
        var codes = TagCodeGenerator.Generate(request, startIndex).ToList();

        // Check for duplicates within batch
        var duplicateCodes = codes
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCodes.Count != 0)
            throw new InvalidOperationException("Duplicate tag codes were generated in the batch.");

        // Check against existing (no DB hit)
        var existingCodeSet = existingCodes.ToHashSet();

        var conflictingCodes = codes
            .Where(c => existingCodeSet.Contains(c))
            .ToList();

        if (conflictingCodes.Any())
            throw new InvalidOperationException(
                $"The following tag codes already exist: {string.Join(", ", conflictingCodes)}");

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
            .ThenInclude(pl => pl!.WorkInstruction).Include(tag => tag.History)
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
            WorkInstructionTitle = workInstructionTitle,
            HasBeenPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed)
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
            .ThenInclude(pl => pl.WorkInstruction).Include(tag => tag.History)
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
            WorkInstructionTitle = lastWorkInstructionTitle,
            HasBeenPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed)
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
            .Include(tag => tag.History)
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
                WorkInstructionTitle = lastWorkInstructionTitle,
                HasBeenPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed)
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
            .Include(tag => tag.History)
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
                WorkInstructionTitle = lastWorkInstructionTitle,
                HasBeenPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed)
            };
        }).ToList();

        return tagDtos;
    }
    
    /// <inheritdoc />
    public async Task<IReadOnlyList<TagDTO>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var tags = await context.Tags
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.PartDefinition)
            .Include(t => t.SerializablePart)
            .ThenInclude(sp => sp!.ProductionLogParts)
            .ThenInclude(plp => plp.ProductionLog)
            .ThenInclude(pl => pl!.WorkInstruction)
            .Include(tag => tag.History)
            .ToListAsync();

        return tags.Select(tag => new TagDTO
        {
            Id = tag.Id,
            Code = tag.Code,
            Status = tag.Status,
            CreatedAt = tag.CreatedAt,
            SerializablePartId = tag.SerializablePartId,
            PartSerialNumber = tag.SerializablePart?.SerialNumber,
            PartName = tag.SerializablePart?.PartDefinition?.Name,
            WorkInstructionTitle = tag.SerializablePart?
                .ProductionLogParts
                .OrderByDescending(plp => plp.ProductionLog?.CreatedOn)
                .FirstOrDefault()?
                .ProductionLog?
                .WorkInstruction?.Title,
            HasBeenPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed)
        }).ToList();
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
    
    /// <inheritdoc />
    public async Task DeleteAsync(int tagId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load the tag with its history and assignment info
        var tag = await context.Tags
            .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} was not found.");

        // Prevent deletion if the tag is assigned, retired, or has been printed
        var hasPrinted = tag.History.Any(h => h.EventType == TagEventType.Printed);
        if (tag.Status != TagStatus.Available || hasPrinted)
        {
            throw new InvalidOperationException(
                "Cannot delete a tag that is assigned, retired, or has already been printed.");
        }

        // Optional: record a deletion history event (audit purposes)
        context.TagHistories.Add(new TagHistory
        {
            TagId = tag.Id,
            EventType = TagEventType.Deleted,
            Timestamp = DateTimeOffset.UtcNow
        });

        // Remove the tag
        context.Tags.Remove(tag);

        // Commit changes
        await context.SaveChangesAsync();
    }
    
    private int DetectNextIndex(
        List<string> codes,
        TagNumberingScheme scheme,
        string? prefix)
    {
        IEnumerable<string> working = codes;

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            working = working
                .Where(c => c.StartsWith(prefix))
                .Select(c => c.Substring(prefix.Length));
        }

        return scheme switch
        {
            TagNumberingScheme.Decimal =>
                working.Select(c => int.TryParse(c, out var x) ? x : -1)
                    .DefaultIfEmpty(-1)
                    .Max() + 1,

            TagNumberingScheme.Hexadecimal =>
                working.Select(c => int.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out var x) ? x : -1)
                    .DefaultIfEmpty(-1)
                    .Max() + 1,

            TagNumberingScheme.Alphanumeric =>
                working.Select(TryFromAlphaSafe)
                    .DefaultIfEmpty(-1)
                    .Max() + 1,

            _ => 0
        };
    }
    
    private static int TryFromAlphaSafe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return -1;

        int result = 0;

        foreach (var c in value.ToUpperInvariant())
        {
            if (c < 'A' || c > 'Z')
                return -1;

            result *= 26;
            result += (c - 'A' + 1);
        }

        return result - 1;
    }
}