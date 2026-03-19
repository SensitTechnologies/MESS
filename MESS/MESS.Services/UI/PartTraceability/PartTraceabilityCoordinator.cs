using MESS.Services.DTOs.ProductionLogs.Form;

namespace MESS.Services.UI.PartTraceability
{
    /// <inheritdoc />
    public class PartTraceabilityCoordinator : IPartTraceabilityCoordinator
    {
        /// <inheritdoc />
        public List<PartTraceabilityOperation> BuildOperations(
            IEnumerable<PartTraceabilitySnapshot> snapshots,
            IReadOnlyDictionary<int, int> logIndexToProductionLogId)
        {
            ArgumentNullException.ThrowIfNull(snapshots);
            ArgumentNullException.ThrowIfNull(logIndexToProductionLogId);

            var operations = new List<PartTraceabilityOperation>();

            foreach (var snapshot in snapshots)
            {
                if (!logIndexToProductionLogId.TryGetValue(snapshot.LogIndex, out var productionLogId))
                {
                    throw new InvalidOperationException(
                        $"No ProductionLogId mapping found for log index {snapshot.LogIndex}.");
                }

                var operation = new PartTraceabilityOperation
                {
                    ProductionLogId = productionLogId,
                    ProducedPartSerialNumber = snapshot.ProducedPartSerialNumber,
                    Entries = snapshot.Entries
                        .Select(e => new PartTraceabilityOperation.PartEntryDTO
                        {
                            PartNodeId = e.PartNodeId,
                            SerialNumber = e.SerialNumber,
                            TagCode = e.TagCode,
                            SerializablePartId = e.SerializablePartId
                        })
                        .ToList()
                };

                operations.Add(operation);
            }

            return operations;
        }
    }
}