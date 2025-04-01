using MESS.Data.Models;

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
}

public class ImportError
{
    public string File { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Import Error with File: {File}. Message: {Message}. Location: {Location}. Error Type: {ErrorType}";
    }
}