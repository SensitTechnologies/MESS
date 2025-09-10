using MESS.Data.Models;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Detail;

/// <summary>
/// Provides extension methods for mapping <see cref="ProductionLogStepAttempt"/> entities
/// into <see cref="StepAttemptDetailDTO"/> objects for read-only display and editing.
/// </summary>
public static class StepAttemptDetailDTOMapper
{
    /// <summary>
    /// Converts a single <see cref="ProductionLogStepAttempt"/> entity into a
    /// <see cref="StepAttemptDetailDTO"/>.
    /// </summary>
    /// <param name="attempt">The production log step attempt entity to convert.</param>
    /// <returns>
    /// A <see cref="StepAttemptDetailDTO"/> containing the mapped attempt details,
    /// including step name, submit time, success status, and optional failure notes.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="attempt"/> is null.</exception>
    public static StepAttemptDetailDTO ToDetailDTO(this ProductionLogStepAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        return new StepAttemptDetailDTO
        {
            AttemptId = attempt.Id,
            StepName = attempt.ProductionLogStep?.WorkInstructionStep?.Name ?? string.Empty,
            SubmitTime = attempt.SubmitTime,
            IsSuccess = attempt.Success ?? false,
            FailureNote = attempt.Success == false ? attempt.Notes : null
        };
    }

    /// <summary>
    /// Converts a collection of <see cref="ProductionLogStepAttempt"/> entities
    /// into a list of <see cref="StepAttemptDetailDTO"/> objects.
    /// </summary>
    /// <param name="attempts">The collection of production log step attempt entities to convert.</param>
    /// <returns>
    /// A list of <see cref="StepAttemptDetailDTO"/> objects representing the mapped attempts.
    /// Returns an empty list if <paramref name="attempts"/> is null.
    /// </returns>
    public static List<StepAttemptDetailDTO> ToDetailDTOList(this IEnumerable<ProductionLogStepAttempt> attempts)
        => attempts?.Select(ToDetailDTO).ToList() ?? [];
}