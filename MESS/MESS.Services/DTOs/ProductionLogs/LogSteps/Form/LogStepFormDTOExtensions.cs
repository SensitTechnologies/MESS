using MESS.Services.DTOs.ProductionLogs.LogSteps.Attempts.Form;

namespace MESS.Services.DTOs.ProductionLogs.LogSteps.Form
{
    /// <summary>
    /// Provides extension methods for <see cref="LogStepFormDTO"/> 
    /// to simplify working with attempts in UI and service layers.
    /// </summary>
    public static class LogStepFormDTOExtensions
    {
        /// <summary>
        /// Gets the most recent attempt for the step, ordered by <see cref="StepAttemptFormDTO.SubmitTime"/>.
        /// </summary>
        /// <param name="logStep">The log step form DTO.</param>
        /// <returns>
        /// The latest <see cref="StepAttemptFormDTO"/> if any exist; 
        /// otherwise <c>null</c>.
        /// </returns>
        public static StepAttemptFormDTO? LatestAttempt(this LogStepFormDTO logStep)
        {
            ArgumentNullException.ThrowIfNull(logStep);

            return logStep.Attempts
                .OrderByDescending(a => a.SubmitTime)
                .FirstOrDefault();
        }
    }
}