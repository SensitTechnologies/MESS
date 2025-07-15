using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.Product;

using Data.Models;
/// <inheritdoc />
public class ProductService : IProductService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;
    /// <summary>
    /// Instantiates a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="contextFactory">The application database context used for data operations.</param>
    public ProductService(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    /// <inheritdoc />
    public async Task DuplicateAsync(Product productToDuplicate)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var newProduct = new Product
            {
                Name = productToDuplicate.Name,
                IsActive = productToDuplicate.IsActive,
                WorkInstructions = new List<WorkInstruction>()
            };

            await context.Products.AddAsync(newProduct);
            await context.SaveChangesAsync();

            if (productToDuplicate.WorkInstructions != null)
            {
                foreach (var workInstruction in productToDuplicate.WorkInstructions)
                {
                    var associatedWorkInstruction = await context.WorkInstructions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(wi => wi.Id == workInstruction.Id);
                    if (associatedWorkInstruction != null)
                    {
                        newProduct.WorkInstructions.Add(associatedWorkInstruction);
                        associatedWorkInstruction.Products.Add(newProduct);
                    }
                    
                }
            }
            
            await context.SaveChangesAsync();
            
            Log.Information("Product: {ProductName} Duplicated Successfully", newProduct.Name);
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while trying to duplicate Product: {ExceptionType}", e.GetType());
        }
    }

    /// <inheritdoc />
    public async Task CreateAsync(Product product)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            
            Log.Information("Product successfully created. ID: {ProductID}", product.Id);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error occured while adding product");
        }
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var product = await context.Products
                .Include(p => p.WorkInstructions)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            Log.Information("Product found. ID: {ProductId}", product?.Id);
            
            return product;
        }
        catch (Exception e)
        {
            Log.Warning(e, "Unable to find product for ID. ID: {InputId}", id);
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetByTitleAsync(string title)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Name == title);
            
            Log.Information("Product found. Title: {ProductTitle}", product?.Name);
            
            return product;
        }
        catch (Exception e)
        {
            Log.Warning(e, "Unable to find product for Title. Title: {InputTitle}", title);
            return null;
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Products
                .Include(p => p.WorkInstructions)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occured while getting all products. Returning empty product list.");
            return new List<Product>();
        }
    }

    /// <inheritdoc />
    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingProduct = await context.Products
                .Include(p => p.WorkInstructions)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct is null)
            {
                Log.Warning("Product with ID {inputId} not found during modification.", product.Id);
                return;
            }

            // Update scalar properties
            existingProduct.Name = product.Name;
            existingProduct.IsActive = product.IsActive;

            // Defensive: ensure list is initialized
            existingProduct.WorkInstructions ??= new List<WorkInstruction>();
            existingProduct.WorkInstructions.Clear();

            if (product.WorkInstructions != null)
            {
                foreach (var wi in product.WorkInstructions)
                {
                    var trackedWi = await context.WorkInstructions.FindAsync(wi.Id);
                    if (trackedWi != null)
                    {
                        existingProduct.WorkInstructions.Add(trackedWi);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occurred while modifying product. Product ID: {inputId}", product.Id);
        }
    }

    /// <inheritdoc />
    public async Task DeleteByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var product = await context.Products.FindAsync(id);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            
            Log.Information("Product successfully removed. ID: {ProductId}", product.Id);
        }
        else
        {
            Log.Warning("Product for removal not found. ID: {InputId}", id);
        }
    }
}