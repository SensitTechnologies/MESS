using MESS.Data.Models;

namespace MESS.Services.DTOs;

/// <summary>
/// Represents a <see cref="SerializablePart"/> that was retrieved from a prior production log
/// and is associated with a specific production log identifier.
/// </summary>
/// <remarks>
/// This record is typically used when loading installed parts from previous production logs
/// into memory for operations such as traceability, inspection, or reuse in new work instructions.
/// </remarks>
/// <param name="ProductionLogId">
/// The unique identifier of the production log from which the part was installed.
/// </param>
/// <param name="Part">
/// The <see cref="SerializablePart"/> instance that was installed in the specified production log.
/// </param>
public sealed record InstalledPartResult(int ProductionLogId, SerializablePart Part);

