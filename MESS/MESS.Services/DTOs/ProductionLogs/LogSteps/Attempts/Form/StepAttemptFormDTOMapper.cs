using MESS.Data.Models;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

/// <summary>
/// Provides mapping functionality between <see cref="ProductionLogStepAttempt"/> entities
/// and <see cref="StepAttemptFormDTO"/> objects for use in client-side form state
/// and server-side persistence.
/// </summary>
public static class StepAttemptFormDTOMapper
{
    /// <summary>
    /// Converts a <see cref="StepAttemptFormDTO"/> into a <see cref="ProductionLogStepAttempt"/> entity.
    /// </summary>
    /// <param name="dto">The form DTO containing attempt data.</param>
    /// <returns>
    /// A <see cref="ProductionLogStepAttempt"/> populated with values from the given <paramref name="dto"/>.
    /// If <see cref="StepAttemptFormDTO.AttemptId"/> is <c>null</c>, the returned entity represents a new attempt.
    /// </returns>
    public static ProductionLogStepAttempt ToEntity(this StepAttemptFormDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new ProductionLogStepAttempt
        {
            Id = dto.AttemptId ?? 0, // EF treats 0 as new
            Success = dto.IsSuccess,
            Notes = dto.FailureNote ?? string.Empty,
            SubmitTime = dto.SubmitTime
        };
    }

    /// <summary>
    /// Converts a <see cref="ProductionLogStepAttempt"/> entity into a <see cref="StepAttemptFormDTO"/>.
    /// </summary>
    /// <param name="attempt">The production log step attempt entity to convert.</param>
    /// <returns>
    /// A <see cref="StepAttemptFormDTO"/> populated with values from the given <paramref name="attempt"/>.
    /// </returns>
    public static StepAttemptFormDTO ToFormDTO(this ProductionLogStepAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        return new StepAttemptFormDTO
        {
            AttemptId = attempt.Id == 0 ? null : attempt.Id,
            IsSuccess = attempt.Success ?? false,
            FailureNote = attempt.Success == false ? attempt.Notes : null,
            SubmitTime = attempt.SubmitTime
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="StepAttemptFormDTO"/>s
    /// into a list of <see cref="ProductionLogStepAttempt"/> entities.
    /// </summary>
    public static List<ProductionLogStepAttempt> ToEntityList(this IEnumerable<StepAttemptFormDTO> dtos)
        => dtos.Select(dto => dto.ToEntity()).ToList();

    /// <summary>
    /// Maps a collection of <see cref="ProductionLogStepAttempt"/> entities
    /// into a list of <see cref="StepAttemptFormDTO"/> objects.
    /// </summary>
    public static List<StepAttemptFormDTO> ToFormDTOList(this IEnumerable<ProductionLogStepAttempt> attempts)
        => attempts.Select(a => a.ToFormDTO()).ToList();
}