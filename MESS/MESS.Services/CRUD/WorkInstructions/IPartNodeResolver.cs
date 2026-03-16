using MESS.Data.Context;
using MESS.Data.Models;

namespace MESS.Services.CRUD.WorkInstructions
{
    /// <summary>
    /// Defines a contract for resolving <see cref="PartNode"/> entities that have
    /// partially specified part information (name and/or number) before saving a work instruction.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are responsible for:
    /// <list type="bullet">
    /// <item>Matching partially specified <see cref="PartNode"/>s to existing <see cref="PartDefinition"/>s.</item>
    /// <item>Creating new <see cref="PartDefinition"/>s if no match exists.</item>
    /// <item>Updating the <see cref="PartNode.PartDefinitionId"/> and/or navigation property to ensure
    /// Entity Framework Core can correctly persist relationships.</item>
    /// <item>Managing its own <see cref="ApplicationContext"/> for database access.</item>
    /// </list>
    /// </remarks>
    public interface IPartNodeResolver
    {
        /// <summary>
        /// Resolves all partially specified <see cref="PartNode"/> entities in a collection
        /// by matching them to existing <see cref="PartDefinition"/>s or creating new ones as needed.
        /// </summary>
        /// <param name="nodes">
        /// A collection of <see cref="WorkInstructionNode"/> entities to resolve.
        /// Only nodes of type <see cref="PartNode"/> with an unresolved <see cref="PartDefinition"/> are affected.
        /// </param>
        /// <param name="context">
        /// The application context passed from a calling service function that needs to resolve part nodes.
        /// </param>
        /// <returns>A <see cref="Task"/> that completes when all nodes have been resolved.</returns>
        Task ResolvePendingNodesAsync(
            ApplicationContext context,
            IEnumerable<WorkInstructionNode> nodes);
    }
}