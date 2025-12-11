using MESS.Data.Models;

namespace MESS.Services.UI.PartTraceability;

/// <summary>
/// Represents the UI input state for a single <see cref="PartNode"/>.
/// Depending on its input type, it may hold a <see cref="SerializablePart"/> or a <see cref="ProductionLog"/> reference.
/// </summary>
public class PartEntry
{
    /// <summary>
    /// Gets or sets the unique identifier of the associated <see cref="PartNode"/>.
    /// </summary>
    public int PartNodeId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PartNode"/> definition that this entry corresponds to.
    /// </summary>
    public PartNode PartNode { get; set; }

    /// <summary>
    /// Gets or sets the serializable part entered for this node, if applicable.
    /// </summary>
    public SerializablePart? SerializablePart { get; set; }

    /// <summary>
    /// Gets or sets the production log linked to this node, if applicable.
    /// </summary>
    public ProductionLog? LinkedProductionLog { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartEntry"/> class for the specified <see cref="PartNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="PartNode"/> that this entry represents.</param>
    public PartEntry(PartNode node)
    {
        PartNode = node;
        PartNodeId = node.Id;
    }
    
    /// <summary>
    /// Creates a deep copy of this <see cref="PartEntry"/> instance.
    /// </summary>
    /// <remarks>
    /// This method copies all scalar properties. Complex reference properties
    /// (such as <see cref="PartDefinition"/>) are copied by reference unless they
    /// implement their own cloning logic. This method is primarily intended
    /// for creating snapshot copies that can be safely modified without
    /// affecting the original <see cref="PartEntry"/>.
    /// </remarks>
    /// <returns>
    /// A new <see cref="PartEntry"/> containing the copied values from the
    /// current instance.
    /// </returns>
    public PartEntry Clone()
    {
        return new PartEntry(PartNode)
        {
            PartNodeId = PartNodeId,
            SerializablePart = SerializablePart == null ? null : new SerializablePart
            {
                Id = SerializablePart.Id,
                PartDefinitionId = SerializablePart.PartDefinitionId,
                PartDefinition = SerializablePart.PartDefinition,
                SerialNumber = SerializablePart.SerialNumber
            },
            LinkedProductionLog = LinkedProductionLog // NOT cloned — this is a reference to an existing log
        };
    }

}