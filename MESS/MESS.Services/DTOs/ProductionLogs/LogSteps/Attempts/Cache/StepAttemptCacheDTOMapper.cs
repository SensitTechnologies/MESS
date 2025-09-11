using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Cache;

/// <summary>
/// Provides mapping between <see cref="StepAttemptFormDTO"/> and <see cref="StepAttemptCacheDTO"/> 
/// for client-side caching purposes.
/// </summary>
public static class StepAttemptCacheDTOMapper
{
    /// <summary>
    /// Maps a <see cref="StepAttemptFormDTO"/> to a <see cref="StepAttemptCacheDTO"/>.
    /// </summary>
    /// <param name="formDto">The form DTO containing step attempt state.</param>
    /// <returns>A <see cref="StepAttemptCacheDTO"/> suitable for storing in browser cache.</returns>
    public static StepAttemptCacheDTO ToCacheDTO(this StepAttemptFormDTO formDto)
    {
        ArgumentNullException.ThrowIfNull(formDto);

        return new StepAttemptCacheDTO
        {
            Success = formDto.IsSuccess,
            FailureNote = formDto.FailureNote,
            SubmitTime = formDto.SubmitTime
        };
    }

    /// <summary>
    /// Maps a <see cref="StepAttemptCacheDTO"/> to a <see cref="StepAttemptFormDTO"/>.
    /// </summary>
    /// <param name="cacheDto">The cached DTO containing step attempt data.</param>
    /// <returns>A <see cref="StepAttemptFormDTO"/> suitable for use in the UI form.</returns>
    public static StepAttemptFormDTO ToFormDTO(this StepAttemptCacheDTO cacheDto)
    {
        ArgumentNullException.ThrowIfNull(cacheDto);

        return new StepAttemptFormDTO
        {
            IsSuccess = cacheDto.Success,
            FailureNote = cacheDto.FailureNote,
            SubmitTime = cacheDto.SubmitTime
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="StepAttemptCacheDTO"/> objects to a list of <see cref="StepAttemptFormDTO"/>.
    /// </summary>
    /// <param name="cacheDtos">The collection of cached DTOs.</param>
    /// <returns>A list of <see cref="StepAttemptFormDTO"/> objects for UI use.</returns>
    public static List<StepAttemptFormDTO> ToFormDTOList(this IEnumerable<StepAttemptCacheDTO> cacheDtos)
        => cacheDtos.Select(dto => dto.ToFormDTO()).ToList();

    /// <summary>
    /// Maps a collection of <see cref="StepAttemptFormDTO"/> objects to a list of <see cref="StepAttemptCacheDTO"/>.
    /// </summary>
    /// <param name="formDtos">The collection of form DTOs.</param>
    /// <returns>A list of <see cref="StepAttemptCacheDTO"/> objects suitable for caching.</returns>
    public static List<StepAttemptCacheDTO> ToCacheDTOList(this IEnumerable<StepAttemptFormDTO> formDtos)
        => formDtos.Select(dto => dto.ToCacheDTO()).ToList();
}
