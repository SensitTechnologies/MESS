using MESS.Services.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace MESS.Services.CRUD.WorkInstructions;
using Data.Models;

/// <summary>
/// Interface for managing work instructions, including operations such as export, import, 
/// retrieval, creation, deletion, and updates. See the WorkInstructionEditor service for more Phoebe
/// specific work instruction editing functions.
/// </summary>
public interface IWorkInstructionService
{
    /// <summary>
    /// Determines whether the specified work instruction is editable.
    /// A work instruction is considered non-editable if it has associated production logs.
    /// </summary>
    /// <param name="workInstruction">The work instruction to evaluate.</param>
    /// <returns>
    /// A boolean value indicating whether the work instruction is editable.
    /// True if editable (i.e., no production logs exist for it); otherwise, false.
    /// </returns>
    public Task<bool> IsEditable(WorkInstruction workInstruction);
    
    /// <summary>
    /// Determines whether a given WorkInstruction is unique based on its properties and contents.
    /// </summary>
    /// <param name="workInstruction">The WorkInstruction to check for uniqueness.</param>
    /// <returns>
    /// True if the WorkInstruction is unique by counting the instances in the database. If there
    /// are 0 or 1 instances of the Title, Version combination it is unique; otherwise, false.
    /// </returns>
    public Task<bool> IsUnique(WorkInstruction workInstruction);

    /// <summary>
    /// Retrieves a List of WorkInstruction objects asynchronously
    /// </summary>
    /// <returns>List of WorkInstruction objects</returns>
    public Task<List<WorkInstruction>> GetAllAsync();
    /// <summary>
    /// Retrieves only the latest versions of all work instructions (IsLatest = true).
    /// </summary>
    Task<List<WorkInstruction>> GetAllLatestAsync();
    /// <summary>
    /// Retrieves all versions of a work instruction lineage given its OriginalId.
    /// </summary>
    /// <param name="originalId">The OriginalId of the lineage.</param>
    /// <returns>List of all versions for that lineage.</returns>
    Task<List<WorkInstruction>> GetVersionHistoryAsync(int originalId);
    /// <summary>
    /// Retrieves a WorkInstruction by its title.
    /// </summary>
    /// <param name="title">The title of the WorkInstruction to retrieve.</param>
    /// <returns>The WorkInstruction if found; otherwise, <c>null</c>.</returns>
    public WorkInstruction? GetByTitle(string title);
    /// <summary>
    /// Retrieves a WorkInstruction by its ID.
    /// </summary>
    /// <param name="id">The ID of the WorkInstruction to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the WorkInstruction if found; otherwise, <c>null</c>.</returns>
    public Task<WorkInstruction?> GetByIdAsync(int id);
    /// <summary>
    /// Creates a WorkInstruction object and saves it to the database.
    /// </summary>
    /// <param name="workInstruction">An instance of WorkInstruction.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> Create(WorkInstruction workInstruction);
    /// <summary>
    /// Deletes a WorkInstruction from the database.
    /// </summary>
    /// <param name="id">The ID of the desired WorkInstruction.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> DeleteByIdAsync(int id);

    /// <summary>
    /// Deletes all versions of a WorkInstruction associated with an id from the database.
    /// </summary>
    /// <param name="id">the id of the starting instruction</param>
    /// <returns></returns>
    public Task<bool> DeleteAllVersionsByIdAsync(int id);

    /// <summary>
    /// Updates a WorkInstruction that is currently saved in the Database.
    /// </summary>
    /// <param name="workInstruction">A WorkInstruction instance.</param>
    /// <returns>A boolean value indicating true for success or false for failure.</returns>
    public Task<bool> UpdateWorkInstructionAsync(WorkInstruction workInstruction);
    
    /// <summary>
    /// Sets IsActive = false for all other versions in this version chain.
    /// </summary>
    /// <param name="workInstructionId"></param>
    /// <returns></returns>
    Task MarkOtherVersionsInactiveAsync(int workInstructionId);

    /// <summary>
    /// Deletes images and other media files associated with the specified <paramref name="nodes"/>.
    /// </summary>
    /// <param name="nodes">The collection of <see cref="WorkInstructionNode"/> entities whose images should be deleted.</param>
    public Task<bool> DeleteNodesAsync(IEnumerable<WorkInstructionNode> nodes);
    
    /// <summary>
    /// Creates a new version of an existing <see cref="WorkInstruction"/> as part of a version lineage.
    /// </summary>
    /// <param name="workInstruction">
    /// The work instruction representing the new version to create. 
    /// This instance must reference an existing version lineage via <see cref="WorkInstruction.OriginalId"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the new version was created successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This operation is transactional and enforces versioning invariants:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// All existing versions in the same lineage are marked as not latest (<see cref="WorkInstruction.IsLatest"/> = <c>false</c>).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The newly created version is marked as the latest version (<see cref="WorkInstruction.IsLatest"/> = <c>true</c>).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The new version is persisted as a distinct database record and does not overwrite prior versions.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="WorkInstruction.OriginalId"/> is <c>null</c>, as versioning requires an existing lineage.
    /// </exception>
    Task<bool> CreateNewVersionAsync(WorkInstruction workInstruction);

}