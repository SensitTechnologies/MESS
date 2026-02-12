using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.Form;

/// <summary>
/// Represents the base class for all work instruction node form DTOs,
/// containing shared metadata and identifiers used by both step and part nodes
/// during creation and editing.
/// </summary>
public abstract class WorkInstructionNodeFormDTO
{
    /// <summary>
    /// Gets or sets the unique identifier of the work instruction node
    /// in the database, if it has been saved.
    /// </summary>
    /// <remarks>
    /// This will be <c>0</c> for new nodes that have not yet been persisted.
    /// </remarks>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a unique client-side identifier used to distinguish nodes
    /// before they are saved to the database.
    /// </summary>
    /// <remarks>
    /// This field is generated on the client (typically as a GUID or temporary key)
    /// and ensures that new, unsaved nodes can be tracked and updated reliably in the UI.
    /// </remarks>
    public Guid ClientId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the position of the node within the work instruction.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the type of node (e.g., <see cref="WorkInstructionNodeType.Step"/>
    /// or <see cref="WorkInstructionNodeType.Part"/>).
    /// </summary>
    public WorkInstructionNodeType NodeType { get; set; }
}