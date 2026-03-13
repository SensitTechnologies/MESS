using MESS.Data.Models;
using MESS.Services.DTOs;
using MESS.Services.DTOs.WorkInstructions.File;
using Microsoft.AspNetCore.Components.Forms;

namespace MESS.Services.Files.WorkInstructions;

/// <summary>
/// Provides functionality for importing and exporting <see cref="WorkInstruction"/> 
/// data between the database and Excel (.xlsx) files, as well as handling 
/// associated media such as step images.
/// </summary>
/// <remarks>
/// This service supports two primary operations:
/// <list type="bullet">
/// <item><description><b>Export:</b> Converts a <see cref="WorkInstruction"/> entity, including its parts and steps, 
/// into a structured Excel workbook with rich text and embedded images.</description></item>
/// <item><description><b>Import:</b> Reads an Excel workbook following the expected schema, 
/// reconstructs a <see cref="WorkInstruction"/> hierarchy, and saves it to the database.</description></item>
/// </list>
/// The expected Excel structure is defined by constant cell and column references at the top of this class.
/// </remarks>
public interface IWorkInstructionFileService
{
    /// <summary>
    /// Exports a work instruction to an Excel (XLSX) file in the same format as import.
    /// </summary>
    /// <param name="workInstructionToExport">
    /// The work instruction to be exported to Excel.
    /// </param>
    /// <param name="progress">
    /// Optional progress reporter used to provide status updates and completion percentage
    /// during the export process. This can be used by UI components (such as a Blazor
    /// progress bar) to display export progress to the user.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token that can be used to cancel the export operation.
    /// If cancellation is requested, the method should attempt to stop processing and return promptly.
    /// </param>
    /// <returns>
    /// A task that resolves to the file path of the generated Excel file if successful;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// The Excel file will include:
    /// - Work instruction title and version in the header
    /// - Product information
    /// - Parts list information
    /// - A table of steps with their titles, descriptions, and references to media files
    ///
    /// The file will be saved with a filename based on the work instruction title and a timestamp.
    ///
    /// If a progress reporter is provided, the export process will emit progress updates
    /// describing major phases of the export, such as:
    /// - Workbook creation
    /// - Header generation
    /// - Step processing
    /// - Image embedding
    /// - File saving
    /// </remarks>
    Task<string?> ExportToXlsxAsync(
        WorkInstructionFileDTO workInstructionToExport,
        IProgress<ExportProgress>? progress = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Imports work instructions from an Excel file.
    /// </summary>
    /// <param name="file">The Excel workbook to be imported.</param>
    /// <returns>
    /// A <see cref="WorkInstructionImportResult"/> object containing:
    /// - Success status
    /// - Imported work instruction (if successful)
    /// - Error details (if failed)
    /// - The names of processed files
    /// </returns>
    /// <remarks>
    /// The Excel file must follow a specific format with cells containing:
    /// - B1: Work instruction title
    /// - D1: Version and QR code requirement
    /// - B2: Product name
    /// - B3: Parts list (format: "(PART_NAME, PART_NUMBER), ...")
    /// - Rows from 7 onwards: Steps with title, description, and media
    /// 
    /// Images found in the Excel file are extracted and saved to the web root directory.
    /// </remarks>
    public Task<WorkInstructionImportResult> ImportFromXlsx(IBrowserFile file);
}