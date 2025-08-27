using MESS.Data.Models;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;

namespace MESS.Services.DTOs.ProductionLogs.CreateRequest;

/// <summary>
/// Provides mapping functionality to convert <see cref="ProductionLogCreateRequest"/> DTOs 
/// into <see cref="ProductionLog"/> entities.
/// </summary>
/// <remarks>
/// This mapper is intended for use during the creation of new production logs. 
/// It initializes core fields, audit metadata, and child <see cref="ProductionLogStep"/> entities 
/// from the provided form DTOs.
/// <para>
/// Navigation properties such as <see cref="Product"/> and <see cref="WorkInstruction"/> are not 
/// attached here in order to avoid Entity Framework Core tracking conflicts. The caller is 
/// responsible for resolving and attaching related entities if needed.
/// </para>
/// </remarks>
public static class ProductionLogCreateRequestMapper
{
    /// <summary>
    /// Maps a <see cref="ProductionLogCreateRequest"/> DTO to a new <see cref="ProductionLog"/> entity.
    /// </summary>
    /// <remarks>
    /// This method does not attach navigation properties like <see cref="Product"/> or <see cref="WorkInstruction"/>.
    /// Instead, only foreign key IDs are assigned where possible to avoid EF Core tracking conflicts.
    /// The caller should load and attach entities if necessary.
    /// </remarks>
    /// <param name="dto">The DTO containing production log data.</param>
    /// <returns>
    /// A new <see cref="ProductionLog"/> entity populated with the data from <paramref name="dto"/>.
    /// Returns <c>null</c> if <paramref name="dto"/> is <c>null</c>.
    /// </returns>
    public static ProductionLog? ToEntity(this ProductionLogCreateRequest? dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var createdBy = dto.CreatedBy ?? string.Empty;
        var now = DateTimeOffset.UtcNow;

        return new ProductionLog
        {
            OperatorId = dto.OperatorId,
            ProductSerialNumber = dto.ProductSerialNumber,
            FromBatchOf = dto.FromBatchOf,
            CreatedBy = createdBy,
            CreatedOn = now,
            LastModifiedBy = createdBy,
            LastModifiedOn = now,
            LogSteps = dto.LogSteps?.Select(stepDto => stepDto.ToEntity()).ToList() ?? []
        };
    }
}