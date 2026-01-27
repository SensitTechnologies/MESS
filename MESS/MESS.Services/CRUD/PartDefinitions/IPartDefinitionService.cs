using MESS.Data.Models;

namespace MESS.Services.CRUD.PartDefinitions;

/// <summary>
/// Defines a set of operations for creating, retrieving, and comparing <see cref="PartDefinition"/> entities.
/// </summary>
/// <remarks>
/// This interface abstracts all data-access operations related to <see cref="PartDefinition"/> records.
/// It supports efficient retrieval by identifiers, part numbers, and associated <see cref="WorkInstruction"/> entities,
/// as well as identifying common parts across work instructions. Implementations are expected to use
/// <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}"/> for safe DbContext creation.
/// </remarks>
public interface IPartDefinitionService
{
    /// <summary>
    /// Retrieves an existing <see cref="PartDefinition"/> from the database that matches
    /// the provided <paramref name="partDefinitionToAdd"/> by name and number.
    /// If no existing record is found, a new one is added and returned.
    /// </summary>
    /// <param name="partDefinitionToAdd">
    /// The <see cref="PartDefinition"/> instance to add if no matching record exists.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the existing or newly created <see cref="PartDefinition"/> entity.
    /// </returns>
    Task<PartDefinition?> GetOrAddPartAsync(PartDefinition partDefinitionToAdd);
    
    /// <summary>
    /// Retrieves an existing <see cref="PartDefinition"/> by name, or creates a new one if no match exists.
    /// </summary>
    /// <param name="name">
    /// The name of the <see cref="PartDefinition"/> to retrieve or create. 
    /// The comparison is case-insensitive.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains:
    /// <list type="bullet">
    ///   <item><description>
    ///   The existing <see cref="PartDefinition"/> if a match is found.
    ///   </description></item>
    ///   <item><description>
    ///   A newly created <see cref="PartDefinition"/> with the specified name (and a <c>null</c> part number)
    ///   if no existing entry is found.
    ///   </description></item>
    ///   <item><description>
    ///   <c>null</c> if the provided name is invalid or an error occurs.
    ///   </description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method performs a case-insensitive search on <see cref="PartDefinition.Name"/>.  
    /// If a new entity is created, its <see cref="PartDefinition.Number"/> property will be set to <c>null</c>.
    /// </remarks>
    Task<PartDefinition?> GetOrCreateByNameAsync(string name);


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
    Task<List<PartDefinition>> GetByWorkInstructionIdAsync(int workInstructionId);
    
    /// <summary>
    /// Retrieves all <see cref="PartDefinition"/> entities from the database.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a list of all <see cref="PartDefinition"/> objects currently stored in the database.
    /// </returns>
    /// <remarks>
    /// This method returns all parts in a detached state (using <c>AsNoTracking</c>),
    /// which means the returned entities are not tracked by the EF Core change tracker.
    /// This is suitable for read-only operations, such as displaying in tables or dropdowns.
    /// </remarks>
    Task<List<PartDefinition>> GetAllAsync();

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
    Task<List<PartDefinition>> GetCommonPartDefinitionsAsync(int workInstructionIdA, int workInstructionIdB);

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
    Task<PartDefinition?> GetByNumberAsync(string number);

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
    Task<PartDefinition?> GetByIdAsync(int id);

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
    /// The returned entities are retrieved using <see cref="Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}(System.Linq.IQueryable{TEntity})"/>
    /// for performance optimization. Duplicate or invalid IDs are automatically ignored.
    /// </remarks>
    Task<List<PartDefinition>> GetByIdsAsync(IEnumerable<int> ids);

    /// <summary>
    /// Checks whether a <see cref="PartDefinition"/> already exists in the database
    /// for the specified part name (optionally number) without creating it.
    /// </summary>
    /// <param name="name">The name of the part to check.</param>
    /// <param name="number">Optional part number to also match.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is <c>true</c> if a matching
    /// part exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(string name, string? number = null);
}