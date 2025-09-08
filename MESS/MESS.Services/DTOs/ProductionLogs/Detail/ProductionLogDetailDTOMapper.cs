using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

namespace MESS.Services.DTOs.ProductionLogs.Detail;

/// <summary>
/// Provides extension methods for mapping <see cref="ProductionLog"/> entities
/// into <see cref="ProductionLogDetailDTO"/> objects for read-only display.
/// </summary>
public static class ProductionLogDetailDTOMapper
{
    /// <summary>
    /// Converts a <see cref="ProductionLog"/> entity into a <see cref="ProductionLogDetailDTO"/>.
    /// </summary>
    /// <param name="log">The production log entity to convert.</param>
    /// <returns>
    /// A <see cref="ProductionLogDetailDTO"/> containing the mapped log details,
    /// including product, work instruction, metadata, and all step attempts.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <c>null</c>.</exception>
    public static ProductionLogDetailDTO ToDetailDTO(this ProductionLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new ProductionLogDetailDTO
        {
            Id = log.Id,
            ProductName = log.Product?.Name ?? string.Empty,
            WorkInstructionName = log.WorkInstruction?.Title ?? string.Empty,
            ProductSerialNumber = log.ProductSerialNumber,
            FromBatchOf = log.FromBatchOf,
            CreatedOn = log.CreatedOn,
            CreatedBy = log.CreatedBy,
            LastModifiedOn = log.LastModifiedOn,
            LastModifiedBy = log.LastModifiedBy,
            Attempts = log.LogSteps
                .SelectMany(step => step.Attempts)
                .Select(attempt => attempt.ToDetailDTO()) // uses extension mapper from StepAttemptDetailDTOMapper
                .ToList()
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="ProductionLog"/> entities into a list of
    /// <see cref="ProductionLogDetailDTO"/> objects.
    /// </summary>
    /// <param name="logs">The collection of production log entities to convert.</param>
    /// <returns>A list of <see cref="ProductionLogDetailDTO"/> objects.</returns>
    public static List<ProductionLogDetailDTO> ToDetailDTOList(this IEnumerable<ProductionLog> logs)
        => logs.Select(l => l.ToDetailDTO()).ToList();
}
