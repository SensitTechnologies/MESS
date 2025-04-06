using System.Text;
using MESS.Data.Models;
using Microsoft.Extensions.Primitives;

namespace MESS.Data.DTO;

public enum ImportStatus
{
    Pending,
    Complete,
    Error
}

public class WorkInstructionImportResult
{
    public bool HasValue { get; set; }
    public List<string> FileNames { get; set; } = [];
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public ImportError? ImportError { get; set; }
    public WorkInstruction? WorkInstruction { get; set; }

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
    
    public static WorkInstructionImportResult Failure(List<string> fileNames)
    {
        return new WorkInstructionImportResult
        {
            HasValue = false,
            FileNames = fileNames,
            Status = ImportStatus.Error
        };
    }
    
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

public class ImportError
{
    public string File { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;

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