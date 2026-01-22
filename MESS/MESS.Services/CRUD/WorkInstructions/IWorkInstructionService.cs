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
    /// Deletes images and other media files associated with the specified <paramref name="nodes"/>.
    /// </summary>
    /// <param name="nodes">The collection of <see cref="WorkInstructionNode"/> entities whose images should be deleted.</param>
    public Task<bool> DeleteNodesAsync(IEnumerable<WorkInstructionNode> nodes);
    
    /// <summary>
    /// Promotes a specific work instruction to be the <c>Active</c> and <c>Latest</c> version within its version chain.
    /// </summary>
    /// <param name="workInstructionId">
    /// The ID of the work instruction to promote. This instruction will become both <c>Active</c> and <c>Latest</c>.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation, containing <c>true</c> if the promotion 
    /// succeeded, or <c>false</c> if the work instruction could not be promoted (for example, if it does not exist).
    /// </returns>
    /// <remarks>
    /// When a work instruction is promoted:
    /// <list type="bullet">
    ///   <item>All other work instructions in the same chain (having the same <c>OriginalId</c>) will be marked inactive and not latest.</item>
    ///   <item>The promoted instruction will have <c>IsActive</c> and <c>IsLatest</c> set to <c>true</c>.</item>
    ///   <item>This method ensures database constraints, such as <c>CK_WorkInstructions_ActiveRequiresLatest</c>, are not violated.</item>
    /// </list>
    /// Use this method instead of manually setting <c>IsActive</c> or <c>IsLatest</c> to maintain chain integrity.
    /// </remarks>
    Task<bool> PromoteVersionAsync(int workInstructionId);

}