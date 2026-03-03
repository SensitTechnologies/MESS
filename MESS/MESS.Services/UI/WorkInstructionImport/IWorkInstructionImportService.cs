using MESS.Services.DTOs.WorkInstructions.File;
using MESS.Services.DTOs.WorkInstructions.Form;
using MESS.Services.Files.WorkInstructions;

namespace MESS.Services.UI.WorkInstructionImport;

/// <summary>
/// Provides application-level logic for importing a 
/// <see cref="WorkInstructionFileDTO"/> into a 
/// <see cref="WorkInstructionFormDTO"/> suitable for editing or persistence.
/// </summary>
/// <remarks>
/// This service is responsible for:
/// <list type="bullet">
/// <item>
/// <description>
/// Resolving name-based references (e.g., products, parts) into database identifiers.
/// </description>
/// </item>
/// <item>
/// <description>
/// Validating that referenced entities exist in the current environment.
/// </description>
/// </item>
/// <item>
/// <description>
/// Transforming file-layer DTOs into UI-layer form DTOs.
/// </description>
/// </item>
/// <item>
/// <description>
/// Reporting validation or resolution errors encountered during import.
/// </description>
/// </item>
/// </list>
/// 
/// This service does <b>not</b> handle file parsing or Excel logic. 
/// That responsibility belongs to <see cref="IWorkInstructionFileService"/>.
/// </remarks>
public interface IWorkInstructionImportService
{
    /// <summary>
    /// Converts a <see cref="WorkInstructionFileDTO"/> into a validated 
    /// <see cref="WorkInstructionFormDTO"/> by resolving all external references.
    /// </summary>
    /// <param name="fileDto">
    /// The file-layer DTO produced by the file import service.
    /// This DTO contains name-based references and is persistence-agnostic.
    /// </param>
    /// <returns>
    /// A <see cref="WorkInstructionImportApplicationResult"/> containing:
    /// <list type="bullet">
    /// <item><description>Success status</description></item>
    /// <item><description>The mapped <see cref="WorkInstructionFormDTO"/> (if successful)</description></item>
    /// <item><description>Error or validation messages (if applicable)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// If any referenced products or parts cannot be resolved, 
    /// the result will indicate failure and include details 
    /// describing the missing references.
    /// </remarks>
    Task<WorkInstructionImportApplicationResult> ImportAsync(WorkInstructionFileDTO fileDto);
}