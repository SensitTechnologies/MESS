namespace MESS.Services.UI.PartTraceability
{
    /// <summary>
    /// A service used for accessing and mutating the current UI state of part traceability operations for a given batch
    /// of production logs.
    /// </summary>
    public interface IPartTraceabilityStateService
    {
        /// <summary>
        /// Initializes the state service with the specified production logs and node blocks.
        /// </summary>
        /// <param name="logIndexes">The log indexes for each production log.</param>
        /// <param name="nodeBlocks">
        /// The ordered list of node blocks (each block is a list of part nodes) from the work instruction.
        /// </param>
        /// <remarks>
        /// Each log index will have a corresponding list of <see cref="PartEntryGroupState"/>
        /// one per node block. Each group will contain entries for the part nodes in that block.
        /// Existing state is cleared.
        /// </remarks>
        void Initialize(IEnumerable<int> logIndexes, IEnumerable<List<int>> nodeBlocks);
        
        /// <summary>
        /// Retrieves a specific <see cref="PartEntryState"/> by log index and part node identifier.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <param name="partNodeId">The unique part node identifier within the log.</param>
        /// <returns>The corresponding <see cref="PartEntryState"/>.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the log or entry does not exist.
        /// </exception>
        PartEntryState GetEntry(int logIndex, int partNodeId);

        /// <summary>
        /// Attempts to retrieve a specific <see cref="PartEntryState"/> by log index and part node identifier.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <param name="partNodeId">The unique part node identifier within the log.</param>
        /// <param name="entry">The resulting entry if found; otherwise null.</param>
        /// <returns>True if the entry exists; otherwise false.</returns>
        bool TryGetEntry(int logIndex, int partNodeId, out PartEntryState? entry);
        
        /// <summary>
        /// Retrieves all part entry states for a specific production log.
        /// </summary>
        /// <param name="logIndex">The zero-based UI index representing the production log.</param>
        /// <returns>
        /// A read-only collection of <see cref="PartEntryState"/> instances for the specified log.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no entries exist for the given <paramref name="logIndex"/>.
        /// </exception>
        IReadOnlyCollection<PartEntryState> GetEntries(int logIndex);
        
        /// <summary>
        /// Attempts to retrieve all part entry states for a specific production log.
        /// </summary>
        /// <param name="logIndex">The zero-based UI index representing the production log.</param>
        /// <param name="entries">
        /// When this method returns, contains a read-only collection of <see cref="PartEntryState"/> instances
        /// if the log exists; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if entries exist for the specified log index; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method provides safe, exception-free access to part entry states.
        /// Use it when you do not want to throw an exception if the log index does not exist.
        /// </remarks>
        bool TryGetEntries(int logIndex, out IReadOnlyCollection<PartEntryState>? entries);

        /// <summary>
        /// Updates the tag code for a specific part entry and attempts to resolve it to a serializable part.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <param name="partNodeId">The unique part node identifier within the log.</param>
        /// <param name="tagCode">The tag code entered by the user.</param>
        /// <returns>
        /// <c>true</c> if the tag code was successfully resolved to a serializable part; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method updates the in-memory <see cref="PartEntryState.TagCode"/> and optionally
        /// sets <see cref="PartEntryState.SerializablePartId"/> if resolution succeeds.
        /// The caller can use the return value to provide UI feedback to the user.
        /// </remarks>
        Task<bool> UpdateTagCodeAsync(int logIndex, int partNodeId, string? tagCode);

        /// <summary>
        /// Updates the serial number for a specific part entry.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <param name="partNodeId">The unique part node identifier within the log.</param>
        /// <param name="serialNumber">The serial number entered by the user.</param>
        void UpdateSerialNumber(int logIndex, int partNodeId, string? serialNumber);
        
        /// <summary>
        /// Removes all state associated with a specific production log index.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <returns>
        /// <c>true</c> if the groups were found and removed; otherwise <c>false</c>.
        /// </returns>
        bool RemoveLog(int logIndex);
        
        /// <summary>
        /// Determines whether state exists for the specified production log index.
        /// </summary>
        bool HasLog(int logIndex);

        /// <summary>
        /// Clears all state managed by the service.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets or clears the produced part serial number for a specific production log.
        /// </summary>
        /// <param name="logIndex">The production log index.</param>
        /// <param name="serialNumber">The serial number to set, or null to clear.</param>
        void SetProducedPartSerialNumber(int logIndex, string? serialNumber);

        /// <summary>
        /// Creates an immutable snapshot of the current part traceability state
        /// for a single production log identified by its UI log index.
        /// </summary>
        /// <param name="logIndex">
        /// The zero-based UI index representing the production log whose state should be captured.
        /// This value is managed by the UI layer and is not a database identifier.
        /// </param>
        /// <returns>
        /// A <see cref="PartTraceabilitySnapshot"/> containing the flattened part entries
        /// and produced part serial number for the specified log index.
        /// </returns>
        /// <remarks>
        /// This method captures the current in-memory state of part traceability inputs,
        /// including serial numbers, tag codes, and any resolved serializable part IDs.
        ///
        /// The returned snapshot is intended for use by higher-level application services,
        /// which may transform it into persistence DTOs (e.g., PartTraceabilityOperation)
        /// by supplying a corresponding ProductionLogId.
        ///
        /// This method does not perform any database access and does not require that
        /// the production log has been persisted.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no part traceability state exists for the specified <paramref name="logIndex"/>.
        /// </exception>
        PartTraceabilitySnapshot CreateSnapshot(int logIndex);
    }
}