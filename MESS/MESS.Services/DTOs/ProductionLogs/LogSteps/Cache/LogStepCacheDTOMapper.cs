using MESS.Services.DTOs.ProductionLogs.LogSteps.Form;
using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Cache;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Cache
{
    /// <summary>
    /// Provides mapping between <see cref="LogStepCacheDTO"/> and <see cref="LogStepFormDTO"/> 
    /// for client-side caching of production log steps.
    /// </summary>
    /// <remarks>
    /// Step mapping delegates attempt conversions to <see cref="StepAttemptCacheDTOMapper"/>.
    /// This class allows UI forms to restore cached step state and persist updated state to the cache.
    /// </remarks>
    public static class LogStepCacheDTOMapper
    {
        /// <summary>
        /// Converts a cached step (<see cref="LogStepCacheDTO"/>) to a form DTO (<see cref="LogStepFormDTO"/>) 
        /// for UI consumption.
        /// </summary>
        /// <param name="cacheDto">The cached step DTO to convert.</param>
        /// <returns>A <see cref="LogStepFormDTO"/> representing the step for use in the UI form.</returns>
        /// <remarks>
        /// The <see cref="Attempts"/> collection is mapped using <see cref="StepAttemptCacheDTOMapper.ToFormDTOList"/>.
        /// </remarks>
        public static LogStepFormDTO ToFormDTO(this LogStepCacheDTO cacheDto)
        {
            ArgumentNullException.ThrowIfNull(cacheDto);

            return new LogStepFormDTO
            {
                WorkInstructionStepId = cacheDto.WorkInstructionStepId,
                ProductionLogStepId = cacheDto.ProductionLogStepId,
                Attempts = cacheDto.Attempts.Select(a => a.ToFormDTO()).ToList() // delegate
            };
        }

        /// <summary>
        /// Converts a form DTO (<see cref="LogStepFormDTO"/>) to a cached step (<see cref="LogStepCacheDTO"/>)
        /// suitable for client-side storage.
        /// </summary>
        /// <param name="formDto">The form DTO to convert.</param>
        /// <returns>A <see cref="LogStepCacheDTO"/> containing the step state for caching.</returns>
        /// <remarks>
        /// The <see cref="Attempts"/> collection is mapped using <see cref="StepAttemptCacheDTOMapper.ToCacheDTOList"/>.
        /// If <see cref="LogStepFormDTO.ProductionLogStepId"/> is null, it defaults to 0 for caching purposes.
        /// </remarks>
        public static LogStepCacheDTO ToCacheDTO(this LogStepFormDTO formDto)
        {
            ArgumentNullException.ThrowIfNull(formDto);

            return new LogStepCacheDTO
            {
                WorkInstructionStepId = formDto.WorkInstructionStepId,
                ProductionLogStepId = formDto.ProductionLogStepId ?? 0, // default to 0 if null
                Attempts = formDto.Attempts.Select(a => a.ToCacheDTO()).ToList() // delegate
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="LogStepCacheDTO"/> objects to a list of <see cref="LogStepFormDTO"/> objects.
        /// </summary>
        /// <param name="dtos">The cached step DTOs to convert.</param>
        /// <returns>A list of form DTOs suitable for UI consumption.</returns>
        public static List<LogStepFormDTO> ToFormDTOList(this IEnumerable<LogStepCacheDTO> dtos)
            => dtos.Select(d => d.ToFormDTO()).ToList();

        /// <summary>
        /// Converts a collection of <see cref="LogStepFormDTO"/> objects to a list of <see cref="LogStepCacheDTO"/> objects
        /// for client-side caching.
        /// </summary>
        /// <param name="dtos">The form DTOs to convert.</param>
        /// <returns>A list of cached step DTOs.</returns>
        public static List<LogStepCacheDTO> ToCacheDTOList(this IEnumerable<LogStepFormDTO> dtos)
            => dtos.Select(d => d.ToCacheDTO()).ToList();
    }
}