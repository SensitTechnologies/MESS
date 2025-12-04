using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.PartDefinitions;

/// <inheritdoc />
public class PartDefinitionService : IPartDefinitionService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartDefinitionService"/> class.
    /// </summary>
    /// <param name="contextFactory">
    /// The <see cref="IDbContextFactory{TContext}"/> used to create instances of the
    /// <see cref="ApplicationContext"/> for database operations.
    /// </param>
    /// <remarks>
    /// This constructor sets up the service for performing create-or-retrieve operations
    /// on <see cref="PartDefinition"/> entities. The provided <paramref name="contextFactory"/>
    /// ensures that each operation uses its own DbContext instance, improving performance
    /// and avoiding tracking conflicts in multithreaded or scoped environments.
    /// </remarks>
    public PartDefinitionService(
        IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public async Task<PartDefinition?> GetOrAddPartAsync(PartDefinition partDefinitionToAdd)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingPart = await context.PartDefinitions.FirstOrDefaultAsync(p =>
                p.Name == partDefinitionToAdd.Name &&
                p.Number == partDefinitionToAdd.Number);


            if (existingPart != null)
            {
                // Detach from context so that EF Core does not attempt to re-add it to the database
                context.Entry(existingPart).State = EntityState.Unchanged;
                Log.Information("GetOrAddPart: Successfully found pre-existing Part with ID: {ExistingPartID}", existingPart.Id);
                return existingPart;
            }
            
            // If a part does not exist in the database create it here
            // and return with database generated ID
            await context.PartDefinitions.AddAsync(partDefinitionToAdd);
            await context.SaveChangesAsync();
            Log.Information("Successfully created a new Part with Name: {PartName}, and Number: {PartNumber}", partDefinitionToAdd.Name, partDefinitionToAdd.Number);
            return partDefinitionToAdd;
        }
        catch (Exception e)
        {
            Log.Warning("Exception when adding part: {Exception}", e.ToString());
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<PartDefinition?> GetOrCreateByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.Warning("GetOrCreateByNameAsync called with empty name.");
            return null;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Case-insensitive match on Name
            var existing = await context.PartDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());

            if (existing != null)
            {
                Log.Information("Found existing PartDefinition '{Name}' (ID {Id})", name, existing.Id);
                return existing;
            }

            // Create a new PartDefinition with only the Name
            var newPart = new PartDefinition
            {
                Name = name,
                Number = null
            };

            context.PartDefinitions.Add(newPart);
            await context.SaveChangesAsync();

            Log.Information("Created new PartDefinition '{Name}' (ID {Id})", name, newPart.Id);

            // Return a clean, detached instance
            context.Entry(newPart).State = EntityState.Detached;
            return newPart;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GetOrCreateByNameAsync for name '{Name}'", name);
            return null;
        }
    }
    
    /// <summary>
    /// Retrieves a list of <see cref="PartDefinition"/> entities that are
    /// common between two specified <see cref="WorkInstruction"/> objects.
    /// </summary>
    /// <param name="workInstructionIdA">
    /// The unique identifier of the first <see cref="WorkInstruction"/> to compare.
    /// </param>
    /// <param name="workInstructionIdB">
    /// The unique identifier of the second <see cref="WorkInstruction"/> to compare.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a list of <see cref="PartDefinition"/> objects that are referenced by both
    /// work instructions through their <see cref="PartNode"/> elements.
    /// </returns>
    /// <remarks>
    /// This method identifies common parts based solely on the <see cref="PartNode.PartDefinition"/>
    /// associations within each work instruction. It does <b>not</b> include parts
    /// referenced by the <see cref="WorkInstruction.PartProduced"/> property.
    /// </remarks>
    public async Task<List<PartDefinition>> GetCommonPartDefinitionsAsync(int workInstructionIdA, int workInstructionIdB)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load PartNodes and their PartDefinitions for both work instructions
        var workInstructionA = await context.WorkInstructions
            .Include(wi => wi.Nodes.OfType<PartNode>())
            .ThenInclude(pn => pn.PartDefinition)
            .FirstOrDefaultAsync(wi => wi.Id == workInstructionIdA);

        var workInstructionB = await context.WorkInstructions
            .Include(wi => wi.Nodes.OfType<PartNode>())
            .ThenInclude(pn => pn.PartDefinition)
            .FirstOrDefaultAsync(wi => wi.Id == workInstructionIdB);

        // Handle nulls defensively
        if (workInstructionA is null || workInstructionB is null)
            return [];

        // Extract all PartDefinitions used in each work instruction
        var partsA = workInstructionA.Nodes
            .OfType<PartNode>()
            .Select(pn => pn.PartDefinition)
            .ToList();

        var partsB = workInstructionB.Nodes
            .OfType<PartNode>()
            .Select(pn => pn.PartDefinition)
            .ToList();

        // Return only common parts (matched by ID)
        var commonParts = partsA
            .Where(pA => partsB.Any(pB => pB.Id == pA.Id))
            .DistinctBy(p => p.Id)
            .ToList();

        return commonParts;
    }
    
    /// <summary>
    /// Retrieves all <see cref="PartDefinition"/> entities that are referenced
    /// by <see cref="PartNode"/> elements within a specified <see cref="WorkInstruction"/>.
    /// </summary>
    /// <param name="workInstructionId">
    /// The unique identifier of the <see cref="WorkInstruction"/> whose part definitions should be retrieved.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a list of <see cref="PartDefinition"/> objects referenced by part nodes
    /// in the specified work instruction.
    /// </returns>
    /// <remarks>
    /// This method returns only part definitions associated with <see cref="PartNode"/> elements.
    /// It does <b>not</b> include the part defined in <see cref="WorkInstruction.PartProduced"/>.
    /// </remarks>
    public async Task<List<PartDefinition>> GetByWorkInstructionIdAsync(int workInstructionId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var workInstruction = await context.WorkInstructions
                .Include(wi => wi.Nodes.OfType<PartNode>())
                    .ThenInclude(pn => pn.PartDefinition)
                .FirstOrDefaultAsync(wi => wi.Id == workInstructionId);

            if (workInstruction is null)
            {
                Log.Warning("WorkInstruction with ID {WorkInstructionId} not found.", workInstructionId);
                return [];
            }

            var partDefinitions = workInstruction.Nodes
                .OfType<PartNode>()
                .Select(pn => pn.PartDefinition)
                .DistinctBy(pd => pd.Id)
                .ToList();

            Log.Information("Retrieved {Count} part definitions for WorkInstruction ID {WorkInstructionId}.",
                partDefinitions.Count, workInstructionId);

            return partDefinitions;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving part definitions for WorkInstruction ID {WorkInstructionId}.", workInstructionId);
            return [];
        }
    }
    
    /// <summary>
    /// Retrieves a single <see cref="PartDefinition"/> entity by its part number.
    /// </summary>
    /// <param name="number">
    /// The part number of the <see cref="PartDefinition"/> to retrieve.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the matching <see cref="PartDefinition"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a case-insensitive lookup based on the <see cref="PartDefinition.Number"/> property.
    /// </remarks>
    public async Task<PartDefinition?> GetByNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            Log.Warning("GetByNumberAsync called with null or empty part number.");
            return null;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var part = await context.PartDefinitions
                .FirstOrDefaultAsync(p => (p.Number ?? "").Equals(number, StringComparison.CurrentCultureIgnoreCase));

            if (part is null)
            {
                Log.Information("No PartDefinition found with Number: {PartNumber}", number);
                return null;
            }

            Log.Information("Retrieved PartDefinition with ID {PartId} for Number {PartNumber}", part.Id, number);
            return part;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving PartDefinition with Number {PartNumber}", number);
            return null;
        }
    }
    
    /// <summary>
    /// Retrieves a single <see cref="PartDefinition"/> entity by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the <see cref="PartDefinition"/> to retrieve.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the matching <see cref="PartDefinition"/> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method performs a direct primary key lookup using the database context
    /// for efficiency. The returned entity is not tracked beyond the scope of the query.
    /// </remarks>
    public async Task<PartDefinition?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            Log.Warning("GetByIdAsync called with an invalid ID: {PartId}", id);
            return null;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var part = await context.PartDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (part is null)
            {
                Log.Information("No PartDefinition found with ID: {PartId}", id);
                return null;
            }

            Log.Information("Retrieved PartDefinition with ID: {PartId} and Number: {PartNumber}", part.Id, part.Number);
            return part;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving PartDefinition with ID: {PartId}", id);
            return null;
        }
    }
    
    /// <summary>
    /// Retrieves a list of <see cref="PartDefinition"/> entities that match the provided identifiers.
    /// </summary>
    /// <param name="ids">
    /// A collection of unique identifiers corresponding to the <see cref="PartDefinition"/> records to retrieve.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// a list of <see cref="PartDefinition"/> entities that match the provided identifiers.
    /// </returns>
    /// <remarks>
    /// This method performs a batched lookup for all provided IDs in a single database query.
    /// The returned entities are retrieved using <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}(IQueryable{TEntity})"/>
    /// for performance optimization. Duplicate or invalid IDs are automatically ignored.
    /// </remarks>
    public async Task<List<PartDefinition>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var enumerable = ids.ToList();
        if (enumerable.Count == 0)
        {
            Log.Warning("GetByIdsAsync called with empty ID list.");
            return [];
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var distinctIds = enumerable.Distinct().Where(id => id > 0).ToList();

            var parts = await context.PartDefinitions
                .AsNoTracking()
                .Where(p => distinctIds.Contains(p.Id))
                .ToListAsync();

            Log.Information("Retrieved {Count} PartDefinitions for {RequestedCount} requested IDs.",
                parts.Count, distinctIds.Count);

            return parts;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving PartDefinitions for provided ID collection.");
            return [];
        }
    }
}