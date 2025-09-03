using MESS.Services.DTOs.ProductionLogs.Cache;
using MESS.Services.DTOs.ProductionLogs.Form;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Cache;

/// <summary>
/// Provides mapping between <see cref="ProductionLogCacheDTO"/> and <see cref="ProductionLogFormDTO"/>.
/// This mapper is primarily used to persist production log state in client-side cache
/// and to restore it for UI form operations.
/// </summary>
public static class ProductionLogCacheDTOMapper
{
    /// <summary>
    /// Maps a <see cref="ProductionLogCacheDTO"/> from browser cache to a <see cref="ProductionLogFormDTO"/> 
    /// suitable for use in the UI form.
    /// </summary>
    /// <param name="cacheDto">The cached DTO representing a production log.</param>
    /// <returns>A <see cref="ProductionLogFormDTO"/> with steps mapped from the cached DTO.</returns>
    /// <remarks>
    /// Properties such as <c>ProductSerialNumber</c> and <c>FromBatchOf</c> are not tracked
    /// in the cache and will be left as default values in the form DTO.
    /// Step mapping is delegated to <see cref="LogStepCacheDTOMapper"/>.
    /// </remarks>
    public static ProductionLogFormDTO ToFormDTO(this ProductionLogCacheDTO cacheDto)
    {
        return new ProductionLogFormDTO
        {
            Id = cacheDto.ProductionLogId,
            LogSteps = cacheDto.LogSteps
                .Select(step => step.ToFormDTO()) // delegate to LogStepCacheDTOMapper
                .ToList()
        };
    }

    /// <summary>
    /// Maps a <see cref="ProductionLogFormDTO"/> to a <see cref="ProductionLogCacheDTO"/> 
    /// suitable for storing in browser cache.
    /// </summary>
    /// <param name="formDto">The form DTO representing a production log in the UI.</param>
    /// <returns>A <see cref="ProductionLogCacheDTO"/> with steps mapped from the form DTO.</returns>
    /// <remarks>
    /// Properties such as <c>ProductSerialNumber</c> and <c>FromBatchOf</c> are not persisted
    /// to the cache. Step mapping is delegated to <see cref="LogStepCacheDTOMapper"/>.
    /// </remarks>
    public static ProductionLogCacheDTO ToCacheDTO(this ProductionLogFormDTO formDto)
    {
        return new ProductionLogCacheDTO
        {
            ProductionLogId = formDto.Id,
            LogSteps = formDto.LogSteps
                .Select(step => step.ToCacheDTO()) // delegate to LogStepCacheDTOMapper
                .ToList()
        };
    }
}
