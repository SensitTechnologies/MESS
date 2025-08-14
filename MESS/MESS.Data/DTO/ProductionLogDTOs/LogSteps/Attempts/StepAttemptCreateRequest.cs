namespace MESS.Data.DTO.ProductionLogDTOs.LogSteps.Attempts
{
    /// <summary>
    /// Represents a request to create a new step attempt for a production log.
    /// Intended for server-side processing when adding attempts that do not yet exist in the database.
    /// </summary>
    public class StepAttemptCreateRequest
    {
        /// <summary>
        /// Gets or sets the identifier of the production log step
        /// that this attempt is associated with.
        /// This must reference an existing step in the database.
        /// </summary>
        public int ProductionLogStepId { get; set; }

        /// <summary>
        /// Gets or sets the outcome of the attempt.
        /// <c>true</c> indicates the step passed, <c>false</c> indicates it failed,
        /// and <c>null</c> may be used if the outcome is undecided at creation.
        /// </summary>
        public bool? IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets any notes associated with the attempt.
        /// Typically used to record failure reasons or supplemental information.
        /// </summary>
        public string? FailureNote { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the attempt was submitted,
        /// including the time zone offset.
        /// </summary>
        public DateTimeOffset SubmitTime { get; set; }
    }
}