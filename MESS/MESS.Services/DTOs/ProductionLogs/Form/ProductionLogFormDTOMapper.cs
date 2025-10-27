using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

namespace MESS.Services.DTOs.ProductionLogs.Form;

/// <summary>
/// Provides mapping functionality between <see cref="ProductionLog"/> entities
/// and <see cref="ProductionLogFormDTO"/> objects for client-side form state
/// and server-side persistence.
/// </summary>
public static class ProductionLogFormDTOMapper
{
    /// <summary>
    /// Converts a <see cref="ProductionLog"/> entity into a <see cref="ProductionLogFormDTO"/>.
    /// </summary>
    /// <param name="log">The production log entity to convert.</param>
    /// <returns>
    /// A <see cref="ProductionLogFormDTO"/> representing the editable state of the production log,
    /// including batch size, product serial number, and all associated steps with attempts.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <c>null</c>.</exception>
    public static ProductionLogFormDTO ToFormDTO(this ProductionLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new ProductionLogFormDTO
        {
            Id = log.Id,
            FromBatchOf = log.FromBatchOf,
            LogSteps = log.LogSteps.ToFormDTOList()
        };
    }

    /// <summary>
    /// Converts a <see cref="ProductionLogFormDTO"/> back into a <see cref="ProductionLog"/> entity.
    /// </summary>
    /// <param name="dto">The form DTO containing production log data.</param>
    /// <returns>
    /// A <see cref="ProductionLog"/> entity populated with values from the form DTO,
    /// including steps and their associated attempts.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <c>null</c>.</exception>
    public static ProductionLog ToEntity(this ProductionLogFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLog
        {
            Id = dto.Id,
            FromBatchOf = dto.FromBatchOf,
            LogSteps = dto.LogSteps.ToEntityList()
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="ProductionLog"/> entities into a list of
    /// <see cref="ProductionLogFormDTO"/>s.
    /// </summary>
    public static List<ProductionLogFormDTO> ToFormDTOList(this IEnumerable<ProductionLog> logs)
        => logs.Select(log => log.ToFormDTO()).ToList();

    /// <summary>
    /// Converts a collection of <see cref="ProductionLogFormDTO"/>s into a list of
    /// <see cref="ProductionLog"/> entities.
    /// </summary>
    public static List<ProductionLog> ToEntityList(this IEnumerable<ProductionLogFormDTO> dtos)
        => dtos.Select(dto => dto.ToEntity()).ToList();
}