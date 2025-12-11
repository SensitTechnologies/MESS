using MESS.Data.Models;
using MESS.Services.DTOs.PartDefinitions;
using MESS.Services.DTOs.WorkInstructions.Summary;

namespace MESS.Services.DTOs.Products.Detail
{
    /// <summary>
    /// Provides mapping functionality between <see cref="Product"/> entities
    /// and <see cref="ProductDetailDTO"/> objects.
    /// </summary>
    public static class ProductDetailDTOMapper
    {
        /// <summary>
        /// Maps a <see cref="Product"/> entity to a <see cref="ProductDetailDTO"/>.
        /// </summary>
        /// <param name="entity">The product entity to map.</param>
        /// <returns>A detailed product DTO populated from the entity.</returns>
        public static ProductDetailDTO ToDetailDTO(this Product entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(entity.PartDefinition);

            return new ProductDetailDTO
            {
                ProductId = entity.Id,
                PartDefinitionId = entity.PartDefinition.Id,
                Number = entity.PartDefinition.Number,
                Name = entity.PartDefinition.Name,
                IsActive = entity.IsActive,
                WorkInstructions = entity.WorkInstructions?.ToSummaryDTOs().ToList() ?? []
            };
        }

        /// <summary>
        /// Maps a collection of <see cref="Product"/> entities to <see cref="ProductDetailDTO"/> objects.
        /// </summary>
        /// <param name="entities">The collection of product entities to map.</param>
        /// <returns>A collection of detailed product DTOs.</returns>
        public static IEnumerable<ProductDetailDTO> ToDetailDTOs(this IEnumerable<Product> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            return entities.Select(e => e.ToDetailDTO());
        }

        /// <summary>
        /// Maps a <see cref="ProductDetailDTO"/> back to a <see cref="Product"/> entity.
        /// </summary>
        /// <param name="dto">The DTO to map from.</param>
        /// <returns>A new <see cref="Product"/> entity populated from the DTO.</returns>
        public static Product ToEntity(this ProductDetailDTO dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new Product
            {
                Id = dto.ProductId,
                PartDefinitionId = dto.PartDefinitionId,
                PartDefinition = new PartDefinition
                {
                    Id = dto.PartDefinitionId,
                    Number = dto.Number,
                    Name = dto.Name
                },
                IsActive = dto.IsActive,
                WorkInstructions = [] // Typically left empty or assigned later
            };
        }

        /// <summary>
        /// Updates an existing <see cref="Product"/> entity with values from a <see cref="ProductDetailDTO"/>.
        /// </summary>
        /// <param name="dto">The DTO containing updated values.</param>
        /// <param name="entity">The entity to update.</param>
        public static void UpdateEntity(this ProductDetailDTO dto, Product entity)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(entity.PartDefinition);

            entity.IsActive = dto.IsActive;
            entity.PartDefinition.Number = dto.Number;
            entity.PartDefinition.Name = dto.Name;
        }
    }
}
