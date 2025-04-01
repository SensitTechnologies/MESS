namespace MESS.Services.Product;

using Data.Models;

public interface IProductService
{
    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddProductAsync(Product product);
    
    /// <summary>
    /// Finds a product by its unique ID, including related WorkInstructions and WorkStations.
    /// </summary>
    /// <param name="id">The ID of the product to find.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> FindProductByIdAsync(int id);
    
    /// <summary>
    /// Finds the 1st product with the given title.
    /// If given a version string it will find the first Product that has the
    /// given Title & Version match.
    ///
    /// Includes related WorkInstructions and WorkStations.
    /// </summary>
    /// <param name="id">The ID of the product to find.</param>
    /// <returns>The product if found; otherwise, null.</returns>
    Task<Product?> FindByTitleAsync(string title, string? version);
    
    /// <summary>
    /// Retrieves all products from the database, including related WorkInstructions and WorkStations.
    /// </summary>
    /// <returns>A list of all products.</returns>
    Task<IEnumerable<Product>> GetAllProductsAsync();
    
    /// <summary>
    /// Updates an existing product in the database.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ModifyProductAsync(Product product);
    
    /// <summary>
    /// Removes a product from the database by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the product to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveProductAsync(int id);
}