using MESS.Data.Models;

namespace MESS.Services.DTOs.WorkInstructions.Version
{
    /// <summary>
    /// Provides mapping functionality between <see cref="WorkInstruction"/> entities
    /// and <see cref="WorkInstructionVersionDTO"/> objects.
    /// </summary>
    public static class WorkInstructionVersionDTOMapper
    {
        /// <summary>
        /// Maps a <see cref="WorkInstruction"/> entity to a <see cref="WorkInstructionVersionDTO"/>.
        /// </summary>
        /// <param name="entity">The work instruction entity to map.</param>
        /// <returns>
        /// A version DTO populated from the entity.
        /// </returns>
        public static WorkInstructionVersionDTO ToVersionDTO(this WorkInstruction entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new WorkInstructionVersionDTO
            {
                Id = entity.Id,
                RootInstructionId = entity.OriginalId ?? entity.Id,
                Version = entity.Version,
                Title = entity.Title,
                LastModifiedOn = entity.LastModifiedOn,
                LastModifiedBy = entity.LastModifiedBy
            };
        }

        /// <summary>
        /// Maps a collection of <see cref="WorkInstruction"/> entities
        /// to <see cref="WorkInstructionVersionDTO"/> objects.
        /// </summary>
        /// <param name="entities">The collection of work instruction entities to map.</param>
        /// <returns>
        /// A collection of version DTOs populated from the entities.
        /// </returns>
        public static IEnumerable<WorkInstructionVersionDTO> ToVersionDTOs(
            this IEnumerable<WorkInstruction> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            return entities.Select(e => e.ToVersionDTO());
        }
    }
}
