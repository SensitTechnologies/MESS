using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using MESS.Services.CRUD.ProductionLogs;
using MESS.Services.Media.WorkInstructions;

namespace MESS.Services.CRUD.WorkInstructions;
using Data.Models;

/// <inheritdoc />
public class WorkInstructionService : IWorkInstructionService
{
    private readonly IProductionLogService _productionLogService;
    private readonly IWorkInstructionImageService _imageService;
    private readonly IMemoryCache _cache;
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    
    private const string WORK_INSTRUCTION_CACHE_KEY = "AllWorkInstructions";
    const string WORK_INSTRUCTION_LATEST_CACHE_KEY = WORK_INSTRUCTION_CACHE_KEY + "_Latest";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkInstructionService"/> class.
    /// </summary>
    /// <param name="productionLogService">The service for managing product-related operations.</param>
    /// <param name="imageService">The service for managing work instruction image-related operations.</param>
    /// <param name="cache">The memory cache for caching work instructions.</param>
    /// <param name="contextFactory">The factory for creating database contexts.</param>
    public WorkInstructionService(IProductionLogService productionLogService, IWorkInstructionImageService imageService, IMemoryCache cache, IDbContextFactory<ApplicationContext> contextFactory)
    {
        _productionLogService = productionLogService;
        _imageService = imageService;
        _cache = cache;
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public async Task<bool> IsEditable(WorkInstruction workInstruction)
    {
        if (workInstruction is not { Id: > 0 })
        {
            Log.Warning("Cannot check editability: Work instruction is null or has invalid ID");
            return false;
        }
        
        try
        {
            await using var localContext = await _contextFactory.CreateDbContextAsync();
            
            // Check if any production logs reference this work instruction. If so the Instruction is not editable.
            var hasProductionLogs = await localContext.ProductionLogs
                .AsNoTracking()
                .AnyAsync(p => p.WorkInstruction != null && p.WorkInstruction.Id == workInstruction.Id);

            return !hasProductionLogs;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to determine if WorkInstruction: {WorkInstructionTitle} is editable. Exception Type: {ExceptionType}", workInstruction.Title, e.GetType());
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsUnique(WorkInstruction workInstruction)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var title = workInstruction.Title;
            var version = workInstruction.Version;

            var isUniqueCount = await context.WorkInstructions
                .CountAsync(w => w.Title == title && w.Version == version);

            if (isUniqueCount >= 1)
            {
                return false;
            }
            
            if (isUniqueCount is 0)
            {
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to determine if WorkInstruction with Title: {Title}, and Version: {Version} is unique. Exception thrown: {Exception}", workInstruction.Title, workInstruction.Version, e.ToString());
            return false;
        }
    }

    /// <summary>
    /// Asynchronously retrieves all work instructions, using caching for performance.
    /// </summary>
    /// <returns>List of work instructions.</returns>
    /// <remarks>
    /// Results are cached for 15 minutes to improve performance.
    /// </remarks>
    public async Task<List<WorkInstruction>> GetAllAsync()
    {
        if (_cache.TryGetValue(WORK_INSTRUCTION_CACHE_KEY, out List<WorkInstruction>? cachedWorkInstructionList) &&
            cachedWorkInstructionList != null)
        {
            return cachedWorkInstructionList;
        }
        
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstructions = await context.WorkInstructions
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ToListAsync();
            
            // Load PartDefinition for all PartNodes (derived entities)
            await context.Set<PartNode>()
                .Include(pn => pn.PartDefinition)
                .LoadAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_CACHE_KEY, workInstructions, TimeSpan.FromMinutes(15));

            Log.Information("GetAllAsync Successfully retrieved a List of {WorkInstructionCount} WorkInstructions", workInstructions.Count);
            return workInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetAllAsync Work Instructions, in WorkInstructionService. Exception: {Exception}", e.ToString());
            return [];
        }
    }

    /// <summary>
    /// Asynchronously retrieves only the latest versions of all work instructions,
    /// using caching for performance.
    /// </summary>
    /// <returns>List of work instructions where IsLatest is true.</returns>
    /// <remarks>
    /// Results are cached for 15 minutes to improve performance.
    /// </remarks>
    public async Task<List<WorkInstruction>> GetAllLatestAsync()
    {
        

        if (_cache.TryGetValue(WORK_INSTRUCTION_LATEST_CACHE_KEY, out List<WorkInstruction>? cachedLatestList) &&
            cachedLatestList != null)
        {
            return cachedLatestList;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
           
            // Load all latest work instructions and their base relationships
            var latestWorkInstructions = await context.WorkInstructions
                .Where(w => w.IsLatest)
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ToListAsync();

            // Explicitly load PartDefinition for derived PartNodes
            await context.Set<PartNode>()
                .Include(pn => pn.PartDefinition)
                .LoadAsync();

            // Cache data for 15 minutes
            _cache.Set(WORK_INSTRUCTION_LATEST_CACHE_KEY, latestWorkInstructions, TimeSpan.FromMinutes(15));

            Log.Information("GetAllLatestAsync successfully retrieved {WorkInstructionCount} latest WorkInstructions", latestWorkInstructions.Count);
            return latestWorkInstructions;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown in GetAllLatestAsync in WorkInstructionService. Exception: {Exception}", e.ToString());
            return [];
        }
    }

    /// <summary>
    /// Retrieves a work instruction by its title.
    /// </summary>
    /// <param name="title">The title of the work instruction to retrieve.</param>
    /// <returns>The work instruction if found, null otherwise.</returns>

    public WorkInstruction? GetByTitle(string title)
    {
        try
        {
            using var context =  _contextFactory.CreateDbContext();
            var workInstruction = context.WorkInstructions
                .FirstOrDefault(w => w.Title.ToUpper() == title.ToUpper());

            Log.Information("Successfully retrieved a WorkInstruction by title: {Title}", title);
            return workInstruction;
        }
        catch (Exception e)
        {
            Log.Warning("Exception: {exceptionType} thrown when attempting to GetByTitle with Title: {title}, in WorkInstructionService", e.GetBaseException().ToString(), title);
            return null;
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves a work instruction by its ID, including related nodes and parts.
    /// </summary>
    /// <param name="id">The ID of the work instruction to retrieve.</param>
    /// <returns>The work instruction with its related data if found, null otherwise.</returns>
    public async Task<WorkInstruction?> GetByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return null;
            }
            
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstruction = await context.WorkInstructions
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).PartDefinition)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            SortNodesByPosition(workInstruction);
            
            return workInstruction;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to GetByIdAsync with ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Ensures that the WorkInstruction's Nodes are ordered by their Position property.
    /// </summary>
    private static void SortNodesByPosition(WorkInstruction? workInstruction)
    {
        if (workInstruction?.Nodes == null || workInstruction.Nodes.Count == 0)
            return;

        workInstruction.Nodes = workInstruction.Nodes
            .OrderBy(n => n.Position)
            .ToList();
    }
    
    /// <summary>
    /// Asynchronously retrieves the full version history for a given work instruction lineage,
    /// identified by its OriginalId. Results are ordered by LastModifiedOn descending (most recent edits first).
    /// </summary>
    /// <param name="originalId">The OriginalId of the work instruction lineage to retrieve.</param>
    /// <returns>
    /// List of all versions in the lineage, including the original itself,
    /// ordered by LastModifiedOn descending.
    /// </returns>
    public async Task<List<WorkInstruction>> GetVersionHistoryAsync(int originalId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var versionHistory = await context.WorkInstructions
                .Where(w => w.OriginalId == originalId || w.Id == originalId)
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).PartDefinition)
                .OrderByDescending(w => w.LastModifiedOn)
                .ToListAsync();

            Log.Information(
                "GetVersionHistoryAsync successfully retrieved {Count} versions for OriginalId {OriginalId}",
                versionHistory.Count,
                originalId
            );

            return versionHistory;
        }
        catch (Exception e)
        {
            Log.Warning(
                "Exception thrown in GetVersionHistoryAsync in WorkInstructionService. Exception: {Exception}",
                e.ToString()
            );
            return [];
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> MarkAllVersionsNotLatestAsync(int originalId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var versions = await context.WorkInstructions
            .Where(w => w.OriginalId == originalId || w.Id == originalId)
            .ToListAsync();

        foreach (var wi in versions)
        {
            wi.IsLatest = false;
        }

        await context.SaveChangesAsync();
        return true;
    }
    
    /// <inheritdoc />
    public async Task MarkOtherVersionsInactiveAsync(int workInstructionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var target = await context.WorkInstructions.FindAsync(workInstructionId);
        if (target == null) return;

        int rootId = target.OriginalId ?? target.Id;

        var chain = await context.WorkInstructions
            .Where(w => (w.Id == rootId || w.OriginalId == rootId) && w.Id != workInstructionId)
            .ToListAsync();

        foreach (var wi in chain)
        {
            wi.IsActive = false;
        }

        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Creates a new work instruction in the database.
    /// </summary>
    /// <param name="workInstruction">The work instruction to create.</param>
    /// <returns>True if creation was successful, false otherwise.</returns>
    /// <remarks>
    /// Validates the work instruction before saving and invalidates the cache after successful creation.
    /// </remarks>
    public async Task<bool> Create(WorkInstruction workInstruction)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            // Validate WorkInstruction
            var workInstructionValidator = new WorkInstructionValidator();
            var validationResult = await workInstructionValidator.ValidateAsync(workInstruction);

            if (!validationResult.IsValid)
            {
                return false;
            }

            // Having to refetch data since we are using DbContextFactory and the change-tracker is reset on each instantiation
            foreach (var product in workInstruction.Products.Where(product => product.Id > 0))
            {
                context.Attach(product);
                context.Entry(product).State = EntityState.Unchanged;
            }
            
            // Attach PartDefinitions for PartNodes
            foreach (var node in workInstruction.Nodes)
            {
                if (node is not PartNode { PartDefinition.Id: > 0 } partNode) continue;
                context.Attach(partNode.PartDefinition);
                context.Entry(partNode.PartDefinition).State = EntityState.Unchanged;
            }
            
            workInstruction.IsActive = false;
            await context.WorkInstructions.AddAsync(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            
            Log.Information("Successfully created WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Create a work instruction, in WorkInstructionService. Exception: {Exception}", e.ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Deletes a Work Instruction from the database if there are no Production log relationships.
    /// </summary>
    /// <param name="id">The ID of the work instruction to delete.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    /// <remarks>
    /// The cache is invalidated after successful deletion.
    /// </remarks>
    public async Task<bool> DeleteByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var workInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (workInstruction == null)
            {
                return false;
            }

            if (!await IsEditable(workInstruction))
            {
                return false;
            }
            
            // Remove associated Work Instruction Node Images first
            await _imageService.DeleteImagesByWorkInstructionAsync(workInstruction);
            
            // Remove associated Work Instruction Nodes first
            context.WorkInstructionNodes.RemoveRange(workInstruction.Nodes);

            context.WorkInstructions.Remove(workInstruction);
            await context.SaveChangesAsync();
            
            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            
            Log.Information("Successfully deleted WorkInstruction with ID: {workInstructionID}", workInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Delete a work instruction with ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
            return false;
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteNodesAsync(IEnumerable<WorkInstructionNode> nodes)
    {
        var nodeList = nodes.ToList();

        if (!nodeList.Any())
            return true;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var nodeIds = nodeList.Select(n => n.Id).ToList();

            var nodesToDelete = await context.WorkInstructionNodes
                .Where(n => nodeIds.Contains(n.Id))
                .ToListAsync();

            if (!nodesToDelete.Any())
                return true;

            // Delegate image deletion to the ImageService
            await _imageService.DeleteImagesByNodesAsync(nodesToDelete);

            context.WorkInstructionNodes.RemoveRange(nodesToDelete);
            await context.SaveChangesAsync();

            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Deleted {Count} WorkInstructionNodes and their images successfully.", nodesToDelete.Count);
            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to delete WorkInstructionNodes and images. Exception: {Exception}", e);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAllVersionsByIdAsync(int id)
    {
        try
        {
            // assuming the input work instruction is the original
            await using var context = await _contextFactory.CreateDbContextAsync();
            var originalWorkInstruction = await context.WorkInstructions
                .Include(w => w.Nodes)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (originalWorkInstruction == null)
            {
                return false;
            }


            // if input work instruction is not the original, find it
            if (originalWorkInstruction.OriginalId != null)
            {
                var ogId = originalWorkInstruction.OriginalId;

                originalWorkInstruction = await context.WorkInstructions
                    .Include(w => w.Nodes)
                    .FirstOrDefaultAsync(w => w.Id == ogId);

                if (originalWorkInstruction == null)
                {
                    return false;
                }
            }
            
            // record original id to get the rest or the versions but delete this one now
            var originalId =  originalWorkInstruction.Id;
            
            // query for all work instructions associated with the original
            var otherVersions = await context.WorkInstructions
                .Where(w => w.OriginalId == originalId)
                .Include(w => w.Nodes)
                .ThenInclude(w => ((PartNode)w).PartDefinition)
                .ToListAsync();
            
            otherVersions.Add(originalWorkInstruction);
            
            // delete each one
            foreach (var version in otherVersions)
            {
                // Remove all production logs associated with a work instruction
                await  _productionLogService.DeleteByWorkInstructionAsync(version);
                await _imageService.DeleteImagesByWorkInstructionAsync(version);
               
                // Remove associated Work Instruction Nodes first
                context.WorkInstructionNodes.RemoveRange(version.Nodes);
                context.WorkInstructions.Remove(version);
            }

            //save
            await context.SaveChangesAsync();

            // Invalidate cache so that on next request users retrieve the latest data
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Successfully deleted all versions associated with WorkInstruction ID: {workInstructionID}", originalWorkInstruction.Id);

            return true;
        }
        catch (Exception e)
        {
            Log.Warning("Exception thrown when attempting to Delete all versions associated with WorkInstruction ID: {id}, in WorkInstructionService. Exception: {Exception}", id, e.ToString());
            return false;
        }
    }

    /// <summary>
    /// Updates an existing work instruction in the database.
    /// </summary>
    /// <param name="workInstruction">The work instruction with updated values.</param>
    /// <returns>True if update was successful, false otherwise.</returns>
    /// <remarks>
    /// Verifies the work instruction exists before updating and invalidates the cache after successful update.
    /// </remarks>
    public async Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction)
    {
        Log.Information("Beginning to update Work Instruction: {Id}", workInstruction.Id);
        if (workInstruction.Id == 0)
            return false;

        await using ApplicationContext context = await _contextFactory.CreateDbContextAsync();

        try
        {
            var existing = await context.WorkInstructions
                .Include(w => w.Products)
                .Include(w => w.Nodes)
                .ThenInclude(n => (n as PartNode)!.PartDefinition)
                .FirstOrDefaultAsync(w => w.Id == workInstruction.Id);

            if (existing == null)
                return false;

            // Enforce uniqueness
            if (!string.Equals(existing.Title, workInstruction.Title, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(existing.Version, workInstruction.Version, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsUnique(workInstruction))
                    return false;
            }

            // Update basic fields
            context.Entry(existing).CurrentValues.SetValues(workInstruction);

            await UpdateProducts(context, existing, workInstruction);
            await UpdateNodes(context, existing, workInstruction);

            await context.SaveChangesAsync();
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");

            Log.Information("Successfully updated WorkInstruction with ID: {Id}", workInstruction.Id);
            return true;
        }
        catch (Exception e)
        {
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY);
            _cache.Remove(WORK_INSTRUCTION_CACHE_KEY + "_Latest");
            Log.Warning("Error updating WorkInstruction {Id}: {Exception}", workInstruction.Id, e);
            return false;
        }
    }

    private static async Task UpdateProducts(DbContext context, WorkInstruction existing, WorkInstruction updated)
    {
        var productIds = updated.Products.Select(p => p.Id).ToHashSet();

        var attachedProducts = await context.Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        existing.Products = attachedProducts;
    }
    
    private async Task UpdateNodes(ApplicationContext context, WorkInstruction existing, WorkInstruction updated)
    {
        var newNodeIds = updated.Nodes.Select(n => n.Id).ToHashSet();

        // Remove deleted nodes
        existing.Nodes.RemoveAll(n => n.Id != 0 && !newNodeIds.Contains(n.Id));

        // Add new nodes
        foreach (var newNode in updated.Nodes.Where(n => n.Id == 0))
            existing.Nodes.Add(newNode);

        // Update existing nodes
        foreach (var newNode in updated.Nodes.Where(n => n.Id != 0))
        {
            var existingNode = existing.Nodes.FirstOrDefault(n => n.Id == newNode.Id);
            if (existingNode == null) continue;

            context.Entry(existingNode).CurrentValues.SetValues(newNode);

            if (existingNode is Step step && newNode is Step newStep)
                UpdateStep(context, step, newStep);

            else if (existingNode is PartNode partNode && newNode is PartNode newPartNode)
                await UpdatePartNode(context, partNode, newPartNode);
        }
    }
    
    private static void UpdateStep(DbContext context, Step existing, Step updated)
    {
        // Merge PrimaryMedia
        foreach (var item in updated.PrimaryMedia)
        {
            if (!existing.PrimaryMedia.Contains(item))
                existing.PrimaryMedia.Add(item);
        }

        // Merge SecondaryMedia
        foreach (var item in updated.SecondaryMedia)
        {
            if (!existing.SecondaryMedia.Contains(item))
                existing.SecondaryMedia.Add(item);
        }

        //remove any images no longer present in the update
        existing.PrimaryMedia = existing.PrimaryMedia
            .Where(m => updated.PrimaryMedia.Contains(m))
            .ToList();
        existing.SecondaryMedia = existing.SecondaryMedia
            .Where(m => updated.SecondaryMedia.Contains(m))
            .ToList();

        // EF will track the changes automatically since these are scalar properties (strings).
        context.Entry(existing).State = EntityState.Modified;
    }
    
    private async Task UpdatePartNode(ApplicationContext context, PartNode existing, PartNode updated)
    {
        Log.Information("Updating PartNode ID {Id}", updated.Id);

        // Update scalar properties on the PartNode itself
        context.Entry(existing).CurrentValues.SetValues(updated);

        // Handle the single PartDefinition relationship
        if (updated.PartDefinition.Id == 0)
        {
            // New PartDefinition – add it
            context.PartDefinitions.Add(updated.PartDefinition);
            existing.PartDefinition = updated.PartDefinition;
        }
        else
        {
            // Existing PartDefinition – attach and update
            var existingPartDef = await context.PartDefinitions.FindAsync(updated.PartDefinition.Id);

            if (existingPartDef != null)
            {
                context.Entry(existingPartDef).CurrentValues.SetValues(updated.PartDefinition);
                existing.PartDefinition = existingPartDef;
            }
            else
            {
                Log.Warning("PartDefinition ID {Id} not found for PartNode {NodeId}", updated.PartDefinition.Id, updated.Id);
                // Optionally attach it anyway if it should exist
                context.Attach(updated.PartDefinition);
                existing.PartDefinition = updated.PartDefinition;
            }
        }

        // Mark node as modified
        context.Entry(existing).State = EntityState.Modified;

        Log.Information(
            "PartNode ID {Id} now has PartDefinition ID {PartDefId}",
            existing.Id,
            existing.PartDefinition.Id
        );
    }
}
