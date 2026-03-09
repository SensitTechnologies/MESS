using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;
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
        ArgumentNullException.ThrowIfNull(dto);

        var logSteps = dto.Attempts?
            .Where(a => a.LogStepId > 0)
            .GroupBy(a => a.LogStepId)
            .Select(group =>
            {
                var attempts = group.ToList();

                return new LogStepUpdateRequest
                {
                    Id = group.Key,
                    WorkInstructionStepId = attempts[0].WorkInstructionStepId,
                    Attempts = attempts.Select(a => a.ToUpdateRequest()).ToList()
                };
            })
            .ToList() ?? [];

        return new ProductionLogUpdateRequest
        {
            Id = dto.Id,
            FromBatchOf = dto.FromBatchOf,
            LogSteps = logSteps
        };
    }
}