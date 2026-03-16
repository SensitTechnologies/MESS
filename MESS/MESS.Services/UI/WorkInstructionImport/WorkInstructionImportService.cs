using MESS.Services.DTOs.WorkInstructions.File;

namespace MESS.Services.UI.WorkInstructionImport;

/// <inheritdoc/>
public class WorkInstructionImportService : IWorkInstructionImportService
{
    /// <inheritdoc/>
    public Task<WorkInstructionImportApplicationResult> ImportAsync(
        WorkInstructionFileDTO fileDto)
    {
        var formDto = fileDto.ToFormDTO();

        return Task.FromResult(new WorkInstructionImportApplicationResult
        {
            Success = true,
            WorkInstruction = formDto
        });
    }
}