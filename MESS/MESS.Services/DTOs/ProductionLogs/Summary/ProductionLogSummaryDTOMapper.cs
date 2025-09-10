using MESS.Data.Models;

namespace MESS.Services.DTOs.ProductionLogs.Summary;

/// <summary>
/// Provides mapping functionality between <see cref="ProductionLog"/> entities
/// and <see cref="ProductionLogSummaryDTO"/> objects.
/// </summary>
public static class ProductionLogSummaryMapper
{
    /// <summary>
    /// Maps a <see cref="ProductionLog"/> entity to a <see cref="ProductionLogSummaryDTO"/>.
    /// </summary>
    /// <param name="entity">The production log entity.</param>
    /// <returns>A summary DTO populated from the entity.</returns>
    public static ProductionLogSummaryDTO ToSummaryDTO(this ProductionLog entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ProductionLogSummaryDTO
        {
            Id = entity.Id,
            ProductName = entity.Product?.Name ?? string.Empty,
            WorkInstructionName = entity.WorkInstruction?.Title ?? string.Empty,
            ProductSerialNumber = entity.ProductSerialNumber,
            CreatedOn = entity.CreatedOn,
            CreatedBy = entity.CreatedBy,
            LastModifiedOn = entity.LastModifiedOn,
            LastModifiedBy = entity.LastModifiedBy
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="ProductionLog"/> entities to summary DTOs.
    /// </summary>
    public static IEnumerable<ProductionLogSummaryDTO> ToSummaryDTOs(this IEnumerable<ProductionLog> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Select(e => e.ToSummaryDTO());
    }
}