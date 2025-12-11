using MESS.Services.DTOs.WorkInstructions.Nodes.Form;

namespace MESS.Services.DTOs.WorkInstructions.Nodes.StepNodes.Form;
    /// <summary>
    /// Represents a form data transfer object (DTO) used for creating or editing
    /// a <see cref="MESS.Data.Models.Step"/> within a work instruction.
    /// </summary>
    /// <remarks>
    /// This DTO is used by the Blazor UI to manage editable step data such as
    /// titles, body content, and media references. It inherits common node
    /// properties from <see cref="WorkInstructionNodeFormDTO"/>.
    /// </remarks>
    public class StepNodeFormDTO : WorkInstructionNodeFormDTO
    {
        /// <summary>
        /// Gets or sets the name or short title of this step.
        /// </summary>
        /// <remarks>
        /// This field is typically displayed as the step’s main label in the UI.
        /// </remarks>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the main body content for this step.
        /// </summary>
        /// <remarks>
        /// This property usually contains the core instructional text or
        /// HTML-formatted content rendered to the operator.
        /// </remarks>
        public required string Body { get; set; }

        /// <summary>
        /// Gets or sets the optional detailed body text for this step.
        /// </summary>
        /// <remarks>
        /// This field can be used for expanded notes or rich HTML
        /// displayed in collapsible UI sections.
        /// </remarks>
        public string? DetailedBody { get; set; }

        /// <summary>
        /// Gets or sets the collection of primary media paths
        /// associated with this step.
        /// </summary>
        /// <remarks>
        /// These are the main media assets (e.g., images)
        /// that appear prominently in the instruction.
        /// </remarks>
        public List<string> PrimaryMedia { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of secondary media file paths
        /// associated with this step.
        /// </summary>
        /// <remarks>
        /// These may include reference images, diagrams, or additional media
        /// used to provide extra context to the operator.
        /// </remarks>
        public List<string> SecondaryMedia { get; set; } = [];
    }