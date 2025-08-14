namespace MESS.Data.DTO.ProductionLogDTOs.LogSteps.Attempts
{
    /// <summary>
    /// Represents an individual step attempt in a production log form submission.
    /// Used for client-to-server communication when saving or updating attempts.
    /// </summary>
    /// <remarks>
    /// This DTO is intended for form state and can represent both new and existing attempts.
    /// New attempts will have <see cref="AttemptId"/> set to <c>null</c>.
    /// </remarks>
    public class StepAttemptFormDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of this attempt.
        /// Will be <c>null</c> if the attempt has not yet been persisted to the database.
        /// </summary>
        public int? AttemptId { get; set; }
        
        /// <summary>
        /// Gets or sets the outcome of this attempt.
        /// <c>true</c> indicates the step passed, <c>false</c> indicates it failed,
        /// and <c>null</c> indicates no outcome has been recorded yet.
        /// </summary>
        public bool? IsSuccess { get; set; }
        
        /// <summary>
        /// Gets or sets the failure note for this attempt, if it was unsuccessful.
        /// Will be <c>null</c> or empty if no note was recorded.
        /// </summary>
        public string? FailureNote { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the attempt was submitted, including the time zone offset.
        /// This value should reflect the local time on the client device when the attempt was recorded.
        /// </summary>
        public DateTimeOffset SubmitTime { get; set; }
    }
}