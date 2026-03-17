using MESS.Data.Models;
using MESS.Services.DTOs.Tags;

namespace MESS.Services.CRUD.Tags;

/// <summary>
/// Defines operations for managing <see cref="Tag"/> entities within the system,
/// including creation, lifecycle management, assignment to serializable parts,
/// lookup, and retrieval of historical events.
/// 
/// This service acts as the primary abstraction for working with tags in a
/// manufacturing execution context, ensuring proper state transitions
/// (e.g., Available, Assigned, Retired) and maintaining traceability through
/// associated <see cref="TagHistory"/> records.
/// 
/// Implementations of this interface are responsible for:
/// <list type="bullet">
/// <item>
/// <description>Enforcing valid tag lifecycle rules (assignment, unassignment, retirement).</description>
/// </item>
/// <item>
/// <description>Recording all significant tag events in history for audit and traceability.</description>
/// </item>
/// <item>
/// <description>Providing efficient lookup and query operations for tags and their associated data.</description>
/// </item>
/// <item>
/// <description>Supporting bulk creation scenarios for large-scale tag generation.</description>
/// </item>
/// </list>
/// </summary>
public interface ITagService
{
    // ---------------------------
    // Creation
    // ---------------------------

    /// <summary>
    /// Creates a single tag.
    /// </summary>
    Task<TagDTO> CreateAsync(TagCreateRequest request);


    /// <summary>
    /// Bulk creates tags based on the specified batch creation request.
    /// This method generates tag codes using the provided numbering scheme and parameters,
    /// persists the tags to the system, and records corresponding creation history events
    /// for traceability.
    /// </summary>
    /// <param name="request">
    /// The batch creation request containing the numbering scheme, prefix, range, and formatting options
    /// used to generate tag codes.
    /// </param>
    /// <returns>
    /// A read-only list of <see cref="TagDTO"/> objects representing the tags that were successfully created.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the request parameters are invalid (e.g., negative count, invalid range, or missing required values).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if one or more generated tag codes already exist in the system and cannot be created.
    /// </exception>
    Task<IReadOnlyList<TagDTO>> BulkCreateAsync(TagBatchCreateRequest request);
    
    // ---------------------------
    // Assignment
    // ---------------------------

    /// <summary>
    /// Assigns a serializable part to a tag.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <param name="serializablePartId">The part being assigned.</param>
    Task<TagDTO> AssignAsync(int tagId, int serializablePartId);

    /// <summary>
    /// Removes the serializable part currently assigned to a tag.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    Task<TagDTO> UnassignAsync(int tagId);

    /// <summary>
    /// Retires a tag permanently so it can no longer be used.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    Task<TagDTO> RetireAsync(int tagId);
    
    /// <summary>
    /// Marks the specified tag as printed by recording a print event in its history.
    /// This does not change the tag's assignment but provides traceability that a physical
    /// label (QR code or barcode) has been produced.
    /// </summary>
    /// <param name="tagId">The unique identifier of the tag to mark as printed.</param>
    /// <returns>
    /// A <see cref="TagDTO"/> representing the updated tag after the print event has been recorded.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if no tag exists with the specified <paramref name="tagId"/>.
    /// </exception>
    Task<TagDTO> MarkPrintedAsync(int tagId);
    
    // ---------------------------
    // Lookup
    // ---------------------------

    /// <summary>
    /// Retrieves a tag by its unique code.
    /// </summary>
    /// <param name="code">The tag code.</param>
    /// <returns>The tag if found.</returns>
    Task<TagDTO?> GetByCodeAsync(string code);

    /// <summary>
    /// Retrieves a tag by its ID.
    /// </summary>
    /// <param name="id">The tag ID.</param>
    Task<TagDTO?> GetByIdAsync(int id);
    
    /// <summary>
    /// Retrieves all tags currently assigned to a serializable part.
    /// </summary>
    Task<IReadOnlyList<TagDTO>> GetBySerializablePartAsync(int serializablePartId);
    
    /// <summary>
    /// Retrieves tags that are currently available for assignment.
    /// </summary>
    Task<IReadOnlyList<TagDTO>> GetAvailableAsync();
    
    /// <summary>
    /// Determines whether a tag with the specified code exists and is currently available for use.
    /// A tag is considered available if it is not assigned to a serializable part and has not been retired.
    /// </summary>
    /// <param name="code">The human-readable tag code to check.</param>
    /// <returns>
    /// <c>true</c> if the tag exists and is available; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsAvailableAsync(string code);
    
    // ---------------------------
    // History
    // ---------------------------

    /// <summary>
    /// Retrieves the history for a specific tag.
    /// </summary>
    Task<IReadOnlyList<TagHistory>> GetHistoryAsync(int tagId);
}