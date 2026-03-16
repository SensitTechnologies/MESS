using MESS.Data.Models;
using MESS.Services.DTOs.Tags;

namespace MESS.Services.CRUD.Tags;

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
    /// Bulk creates tags using the provided tag codes.
    /// </summary>
    /// <param name="codes">The tag codes to create.</param>
    /// <returns>The created tags.</returns>
    Task<IReadOnlyList<Tag>> BulkCreateAsync(IEnumerable<string> codes);
    
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
    
    Task<bool> IsAvailableAsync(string code);
    
    // ---------------------------
    // History
    // ---------------------------

    /// <summary>
    /// Retrieves the history for a specific tag.
    /// </summary>
    Task<IReadOnlyList<TagHistory>> GetHistoryAsync(int tagId);
}