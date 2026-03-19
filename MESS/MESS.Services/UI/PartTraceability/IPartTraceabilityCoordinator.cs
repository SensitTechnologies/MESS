using MESS.Services.DTOs.ProductionLogs.Form;

namespace MESS.Services.UI.PartTraceability
{
    /// <summary>
    /// Coordinates the transformation of UI-level part traceability state into
    /// persistence-ready operations, and provides utilities for validation and
    /// change detection.
    /// </summary>
    /// <remarks>
    /// This service operates on <see cref="PartTraceabilitySnapshot"/> instances,
    /// which represent the current UI state of part traceability inputs.
    ///
    /// It is responsible for:
    /// <list type="bullet">
    /// <item>
    /// Transforming snapshots into <see cref="PartTraceabilityOperation"/> DTOs
    /// using a provided mapping of UI log indexes to database production log IDs.
    /// </item>
    /// <item>
    /// Determining whether snapshots have changed compared to a previous state.
    /// </item>
    /// <item>
    /// Performing lightweight validation to ensure data is safe for persistence.
    /// </item>
    /// </list>
    ///
    /// This service does not perform any database access and does not manage UI state directly.
    /// </remarks>
    public interface IPartTraceabilityCoordinator
    {
        /// <summary>
        /// Builds a collection of <see cref="PartTraceabilityOperation"/> instances
        /// from the provided snapshots and log index to production log ID mapping.
        /// </summary>
        /// <param name="snapshots">
        /// The collection of UI-derived snapshots representing the current part traceability state.
        /// </param>
        /// <param name="logIndexToProductionLogId">
        /// A mapping of UI log indexes to their corresponding database production log IDs.
        /// </param>
        /// <returns>
        /// A list of <see cref="PartTraceabilityOperation"/> objects ready for persistence.
        /// </returns>
        /// <remarks>
        /// Each snapshot must have a corresponding production log ID in the provided mapping.
        /// This method performs a pure transformation and does not persist any data.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="snapshots"/> or <paramref name="logIndexToProductionLogId"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a snapshot does not have a corresponding production log ID in the mapping.
        /// </exception>
        List<PartTraceabilityOperation> BuildOperations(
            IEnumerable<PartTraceabilitySnapshot> snapshots,
            IReadOnlyDictionary<int, int> logIndexToProductionLogId);
    }
}