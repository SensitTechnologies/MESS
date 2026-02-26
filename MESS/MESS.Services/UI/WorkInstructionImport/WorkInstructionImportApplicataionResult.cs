using MESS.Services.DTOs.WorkInstructions.Form;

namespace MESS.Services.UI.WorkInstructionImport;

/// <summary>
/// Represents the outcome of an application-level work instruction import.
/// </summary>
public class WorkInstructionImportApplicationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the import completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the resulting form DTO if the import was successful.
    /// </summary>
    public WorkInstructionFormDTO? WorkInstruction { get; set; }

    /// <summary>
    /// Gets or sets any validation or resolution errors encountered during import.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}