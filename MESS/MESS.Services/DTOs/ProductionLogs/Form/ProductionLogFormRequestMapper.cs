using MESS.Services.DTOs.ProductionLogs.CreateRequest;
using MESS.Services.DTOs.ProductionLogs.UpdateRequest;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

namespace MESS.Services.DTOs.ProductionLogs.Form;

/// <summary>
/// Provides mapping functionality for converting 
/// <see cref="ProductionLogFormDTO"/> objects into 
/// create or update request DTOs.
/// </summary>
public static class ProductionLogFormRequestMapper
{
    /// <summary>
    /// Converts a <see cref="ProductionLogFormDTO"/> into a 
    /// <see cref="ProductionLogCreateRequest"/>.
    /// Intended for when a new log is created from the UI.
    /// </summary>
    public static ProductionLogCreateRequest ToCreateRequest(this ProductionLogFormDTO dto, string createdBy, string? operatorId, int productId, int workInstructionId)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLogCreateRequest
        {
            ProductId = productId,
            WorkInstructionId = workInstructionId,
            ProductSerialNumber = dto.ProductSerialNumber,
            FromBatchOf = dto.FromBatchOf,
            OperatorId = operatorId,
            CreatedBy = createdBy,
            LogSteps = dto.LogSteps // ✅ already a List<LogStepFormDTO>, no conversion needed
        };
    }

    /// <summary>
    /// Converts a <see cref="ProductionLogFormDTO"/> into a 
    /// <see cref="ProductionLogUpdateRequest"/>.
    /// Intended for when an existing log is updated from the UI.
    /// </summary>
    public static ProductionLogUpdateRequest ToUpdateRequest(this ProductionLogFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLogUpdateRequest
        {
            Id = dto.Id,
            ProductSerialNumber = dto.ProductSerialNumber,
            FromBatchOf = dto.FromBatchOf,
            LogSteps = dto.LogSteps
                .Select(step => step.ToUpdateRequest()) // DTO → UpdateRequest
                .ToList()
        };
    }
}
