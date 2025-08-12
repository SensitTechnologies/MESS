using MESS.Data.Models;

namespace MESS.Services.ProductionLogPartService;


/// <summary>
/// Represents a collection of <see cref="ProductionLogPart"/> entries associated with a specific <see cref="PartNode"/>.
/// Used to track serial number log entries per node within a production log session.
/// </summary>
public class PartNodeLogEntry
{
    /// <summary>
    /// Gets or sets the ID of the <see cref="PartNode"/> this entry is associated with.
    /// </summary>
    public int PartNodeId { get; set; }
    
    /// <summary>
    /// Gets or sets the list of <see cref="ProductionLogPart"/> entries linked to this <see cref="PartNode"/>.
    /// </summary>
    public List<ProductionLogPart> LogParts { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PartNodeLogEntry"/> class with the specified <see cref="PartNode"/> ID.
    /// </summary>
    /// <param name="partNodeId">The ID of the <see cref="PartNode"/> this entry will represent.</param>
    public PartNodeLogEntry(int partNodeId)
    {
        PartNodeId = partNodeId;
    }
}