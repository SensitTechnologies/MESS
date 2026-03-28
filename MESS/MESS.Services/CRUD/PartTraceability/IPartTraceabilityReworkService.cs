using MESS.Services.UI.PartTraceability;

namespace MESS.Services.CRUD.PartTraceability;

/// <summary>
/// Defines operations for reconstructing part traceability snapshots from existing serialized parts
/// using scanned tag codes and a work instruction structure.
/// </summary>
public interface IPartTraceabilityReworkService
{
    /// <summary>
    /// Builds a collection of <see cref="PartTraceabilitySnapshot"/> instances from the provided tag codes.
    /// Each tag code represents a produced (root) part for a log entry. The method traverses the underlying
    /// serialized part hierarchy and populates child entries based on the structure defined by the specified
    /// work instruction.
    /// </summary>
    /// <param name="tagCodes">
    /// A list of tag codes corresponding to produced parts. Each tag maps to a single snapshot/log index.
    /// </param>
    /// <param name="workInstructionId">
    /// The identifier of the work instruction used to determine which part nodes should be included
    /// in the resulting snapshots.
    /// </param>
    /// <returns>
    /// A list of <see cref="PartTraceabilitySnapshot"/> objects, one per valid tag code,
    /// representing reconstructed traceability data.
    /// </returns>
    Task<List<PartTraceabilitySnapshot>> BuildSnapshotsFromTagCodesAsync(
        List<string> tagCodes,
        int workInstructionId);
}