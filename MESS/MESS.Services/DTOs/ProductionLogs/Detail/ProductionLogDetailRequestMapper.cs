using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.UpdateRequest;
using MESS.Services.DTOs.ProductionLogs.LogSteps.UpdateRequest;
using MESS.Services.DTOs.ProductionLogs.UpdateRequest;

namespace MESS.Services.DTOs.ProductionLogs.Detail;

/// <summary>
/// Provides mapping extensions to convert a <see cref="ProductionLogDetailDTO"/>
/// into a <see cref="ProductionLogUpdateRequest"/> suitable for persistence.
/// </summary>
/// <remarks>
/// This mapper is used primarily in admin/detail views, where attempts are stored
/// in a flattened structure within the <see cref="ProductionLogDetailDTO"/>.
/// It groups those attempts by their parent log step before constructing the update request.
/// </remarks>
public static class ProductionLogDetailRequestMapper
{
    /// <summary>
    /// Maps a <see cref="ProductionLogDetailDTO"/> into a <see cref="ProductionLogUpdateRequest"/>.
    /// </summary>
    /// <param name="dto">The production log detail DTO to map from.</param>
    /// <returns>
    /// A <see cref="ProductionLogUpdateRequest"/> representing the changes
    /// captured in the detail DTO, grouped by log steps and their associated attempts.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static ProductionLogUpdateRequest ToUpdateRequest(this ProductionLogDetailDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        // Group all flattened attempts back into their parent log steps
        var logSteps = dto.Attempts
            .GroupBy(a => a.LogStepId)
            .Select(group =>
            {
                var first = group.First();

                return new LogStepUpdateRequest
                {
                    Id = group.Key,
                    WorkInstructionStepId = first.WorkInstructionStepId,
                    Attempts = group
                        .Select(a => a.ToUpdateRequest()) // uses StepAttemptDetailRequestMapper
                        .ToList()
                };
            })
            .ToList();

        // Map top-level production log fields
        return new ProductionLogUpdateRequest
        {
            Id = dto.Id,
            ProductSerialNumber = dto.ProductSerialNumber,
            FromBatchOf = dto.FromBatchOf,
            LogSteps = logSteps
        };
    }
}