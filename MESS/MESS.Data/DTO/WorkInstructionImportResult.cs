using System.Text;
using MESS.Data.Models;
using Microsoft.Extensions.Primitives;

namespace MESS.Data.DTO;

/// <summary>
/// Represents the status of an import operation.
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// The import operation is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The import operation completed successfully.
    /// </summary>
    Complete,

    /// <summary>
    /// The import operation encountered an error.
    /// </summary>
    Error
}

/// <summary>
/// Represents the result of a work instruction import operation.
/// </summary>
public class WorkInstructionImportResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the result has a value.
    /// </summary>
    public bool HasValue { get; set; }

    /// <summary>
    /// Gets or sets the list of file names associated with the import operation.
    /// </summary>
    public List<string> FileNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the status of the import operation.
    /// <remarks>
    /// Default ImportStatus is Pending
    /// </remarks>
    /// </summary>
    public ImportStatus Status { get; set; } = ImportStatus.Pending;

    /// <summary>
    /// Gets or sets the error details if the import operation failed.
    /// </summary>
    public ImportError? ImportError { get; set; }

    /// <summary>
    /// Gets or sets the work instruction associated with the import operation.
    /// </summary>
    public WorkInstruction? WorkInstruction { get; set; }

    /// <summary>
    /// Creates a successful result for a work instruction import operation.
    /// </summary>
    /// <param name="fileNames">The list of file names associated with the import.</param>
    /// <param name="instruction">The successfully imported work instruction.</param>
    /// <returns>A <see cref="WorkInstructionImportResult"/> indicating success.</returns>
    public static WorkInstructionImportResult Success(List<string> fileNames, WorkInstruction instruction)
    {
        return new WorkInstructionImportResult
        {
            HasValue = true,
            FileNames = fileNames,
            WorkInstruction = instruction,
            Status = ImportStatus.Complete
        };
    }

    /// <summary>
    /// Creates a failure result for a work instruction import operation.
    /// </summary>
    /// <param name="fileNames">The list of file names associated with the failed import.</param>
    /// <returns>A <see cref="WorkInstructionImportResult"/> indicating failure.</returns>
    public static WorkInstructionImportResult Failure(List<string> fileNames)
    {
        return new WorkInstructionImportResult
        {
            HasValue = false,
            FileNames = fileNames,
            Status = ImportStatus.Error
        };
    }

    /// <summary>
    /// Creates a result indicating that no files were provided for the import operation.
    /// </summary>
    /// <returns>A <see cref="WorkInstructionImportResult"/> indicating an error due to missing files.</returns>
    public static WorkInstructionImportResult NoFilesProvided()
    {
        return new WorkInstructionImportResult
        {
            HasValue = false,
            ImportError = new ImportError
            {
                File = "N/A",
                Message = "No files provided. Please try again."
            },
            Status = ImportStatus.Error
        };
    }

    /// <summary>
    /// Creates a result indicating that no product was found for the given file and product title.
    /// </summary>
    /// <param name="file">The name of the file associated with the error.</param>
    /// <param name="productTitle">The title of the product that was not found.</param>
    /// <returns>A <see cref="WorkInstructionImportResult"/> indicating an error due to a missing product.</returns>
    public static WorkInstructionImportResult NoProductFound(string file, string productTitle)
    {
        return new WorkInstructionImportResult
        {
            HasValue = false,
            ImportError = new ImportError
            {
                File = file,
                Message = $"No Product found with title: {productTitle}. Ensure that this product title exists in the database."
            },
            Status = ImportStatus.Error
        };
    }

    /// <summary>
    /// Creates a result indicating that a duplicate work instruction was found in the database.
    /// </summary>
    /// <param name="file">The name of the file associated with the error.</param>
    /// <param name="workInstructionTitle">The title of the duplicate work instruction.</param>
    /// <param name="workInstructionVersion">The version of the duplicate work instruction.</param>
    /// <returns>A <see cref="WorkInstructionImportResult"/> indicating an error due to a duplicate work instruction.</returns>
    public static WorkInstructionImportResult DuplicateWorkInstructionFound(string file, string workInstructionTitle, string workInstructionVersion)
    {
        return new WorkInstructionImportResult
        {
            HasValue = false,
            ImportError = new ImportError
            {
                File = file,
                Message = $"There exists a work instruction in the database with the same Title: {workInstructionTitle} and Version: {workInstructionVersion}. Please ensure you are creating a unique Work Instruction, by changing the Title and/or Version."
            },
            Status = ImportStatus.Error
        };
    }
}

/// <summary>
/// A class representation of an error that occurred during an import operation.
/// </summary>
public class ImportError
{
    /// <summary>
    /// Gets or sets the name of the file associated with the error.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message describing the issue.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location of file where the error occurred.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the error.
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Returns a string representation of the import error, including details about the file, message, location, and error type.
    /// </summary>
    /// <returns>A string describing the import error.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Import Error with File: ");
        sb.Append(File);
        
        if (!string.IsNullOrEmpty(Message))
        {
            sb.Append($". {Message}");
        }
        
        if (!string.IsNullOrEmpty(Location))
        {
            sb.Append($". Location: {Location}");
        }
        
        if (!string.IsNullOrEmpty(ErrorType))
        {
            sb.Append($". Error Type: {ErrorType}");
        }
        
        return sb.ToString();
    }
}