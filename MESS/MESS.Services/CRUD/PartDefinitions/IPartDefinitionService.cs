using MESS.Data.Models;

namespace MESS.Services.CRUD.PartDefinitions
{
    /// <summary>
    /// Defines operations for retrieving, creating, and managing <see cref="PartDefinition"/> records.
    /// </summary>
    /// <remarks>
    /// This service provides access to part definition data used across products, 
    /// work instructions, and traceability operations.
    /// </remarks>
    public interface IPartDefinitionService
    {
        /// <summary>
        /// Retrieves an existing <see cref="PartDefinition"/> from the database 
        /// or creates a new one if it does not already exist.
        /// </summary>
        /// <param name="partDefinitionToAdd">
        /// The part definition object to retrieve or create. 
        /// Both <see cref="PartDefinition.Name"/> and <see cref="PartDefinition.Number"/> 
        /// are used to determine uniqueness.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the persisted <see cref="PartDefinition"/> from the database, 
        /// or <c>null</c> if an error occurs.
        /// </returns>
        Task<PartDefinition?> GetOrAddPartAsync(PartDefinition partDefinitionToAdd);
    }
}