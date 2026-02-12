using MESS.Services.DTOs.Products.Summary;

namespace MESS.Services.CRUD.Products;

using Data.Models;

/// <summary>
/// Interface for managing product-related operations in the database.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a duplicate of the specified product in the database.
    /// </summary>
    /// <param name="productToDuplicate">The product to duplicate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DuplicateAsync(Product productToDuplicate);

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAsync(Product product);

    /// <summary>
    /// Finds a product by its unique ID, including related WorkInstructions and WorkStations.
    /// </summary>
    /// <param name="id">The ID of the product to find.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Finds the 1st product with the given title.
    /// If given a version string it will find the first Product that has the
    /// given Title and Version match.
    /// Includes related WorkInstructions and WorkStations.
    /// </summary>
    /// <param name="title">The ID of the product to find.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> GetByTitleAsync(string title);
    
    /// <summary>
    /// Retrieves all products from the database, including related WorkInstructions and WorkStations.
    /// </summary>
    /// <returns>A list of all products.</returns>
    Task<IEnumerable<Product>> GetAllAsync();
    
    /// <summary>
    /// Asynchronously retrieves all products as lightweight summary DTOs,
    /// suitable for display in lists or selection controls.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ProductSummaryDTO"/> objects representing all products.
    /// Each DTO contains the product ID, name, number, active status, and associated part definition ID.
    /// </returns>
    /// <remarks>
    /// Only the <see cref="Product.PartDefinition"/> relationship is loaded to populate
    /// the DTO's Name and Number fields. This method is optimized for read-only projections.
    /// Exceptions during retrieval are caught and logged, with an empty list returned on failure.
    /// </remarks>
    Task<IEnumerable<ProductSummaryDTO>> GetAllSummariesAsync();

    /// <summary>
    /// Updates an existing product in the database.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateProductAsync(Product product);

    /// <summary>
    /// Removes a product from the database by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the product to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByIdAsync(int id);

    /// <summary>
    /// Associates additional work instructions with the specified product,
    /// without removing existing associations.
    /// </summary>
    /// <param name="productId">The ID of the product to update.</param>
    /// <param name="workInstructionIds">A list of work instruction IDs to associate.</param>
    Task AddWorkInstructionsAsync(int productId, List<int> workInstructionIds);

    /// <summary>
    /// Removes specific work instruction associations from a product.
    /// </summary>
    /// <param name="productId">The ID of the product to update.</param>
    /// <param name="workInstructionIds">A list of work instruction IDs to remove.</param>
    Task RemoveWorkInstructionsAsync(int productId, List<int> workInstructionIds);
}