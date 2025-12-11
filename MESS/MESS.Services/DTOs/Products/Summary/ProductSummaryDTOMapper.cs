namespace MESS.Services.DTOs.Products.Summary;

using System;
using System.Collections.Generic;
using System.Linq;
using Data.Models;

/// <summary>
/// Provides extension methods for mapping between <see cref="Product"/> and <see cref="ProductSummaryDTO"/>.
/// Used exclusively for read-only projections and summaries.
/// </summary>
public static class ProductSummaryDTOMapper
{
    /// <summary>
    /// Maps a <see cref="Product"/> entity to a <see cref="ProductSummaryDTO"/>.
    /// </summary>
    /// <param name="product">The product to map.</param>
    /// <returns>A <see cref="ProductSummaryDTO"/> instance.</returns>
    public static ProductSummaryDTO ToSummaryDTO(this Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        return new ProductSummaryDTO
        {
            ProductId = product.Id,
            PartDefinitionId = product.PartDefinitionId,
            Name = product.PartDefinition?.Name ?? string.Empty,
            Number = product.PartDefinition?.Number ?? string.Empty,
            IsActive = product.IsActive
        };
    }

    /// <summary>
    /// Maps a collection of <see cref="Product"/> entities to a list of <see cref="ProductSummaryDTO"/> objects.
    /// </summary>
    /// <param name="products">The product collection to map.</param>
    /// <returns>A list of <see cref="ProductSummaryDTO"/> objects.</returns>
    public static List<ProductSummaryDTO> ToSummaryDTOList(this IEnumerable<Product> products)
    {
        if (products is null)
            throw new ArgumentNullException(nameof(products));

        return products.Select(p => p.ToSummaryDTO()).ToList();
    }
}

