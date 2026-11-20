using MESS.Services.DTOs.Rework;

namespace MESS.Services.CRUD.Rework;

/// <summary>
/// Lists production log step attempts and applies Technician/Administrator corrections to them.
/// </summary>
public interface IReworkService
{
    /// <summary>Returns one page of attempts matching the query.</summary>
    Task<ReworkPageDTO> GetPageAsync(ReworkQuery query);

    /// <summary>Distinct values used as filter suggestions.</summary>
    Task<ReworkFilterOptionsDTO> GetFilterOptionsAsync();

    /// <summary>Loads one attempt plus its parent context (log id, sibling counts, has-parts flag).</summary>
    Task<ReworkEditDTO?> GetForEditAsync(int attemptId);

    /// <summary>Step nodes of an instruction, ordered by Position (Step nodes only).</summary>
    Task<IReadOnlyList<ReworkStepOptionDTO>> GetStepsForInstructionAsync(int workInstructionId);

    /// <summary>
    /// Applies a correction atomically. Rejects inconsistent or stale edits and callers who are
    /// not Technicians or Administrators.
    /// </summary>
    /// <param name="request">The correction.</param>
    /// <param name="modifiedBy">User name of the caller; stamped on the owning log.</param>
    Task<ReworkUpdateResult> UpdateAsync(ReworkUpdateRequest request, string modifiedBy);
}
