using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.Product;

using Data.Models;

public class ProductService : IProductService
{
    private readonly ApplicationContext _context;

    public ProductService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task DuplicateProductAsync(Product productToDuplicate)
    {
        try
        {
            _context.ChangeTracker.Clear();
            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var newProduct = new Product
                    {
                        Name = productToDuplicate.Name,
                        IsActive = productToDuplicate.IsActive,
                        WorkInstructions = new List<WorkInstruction>()
                    };

                    await _context.Products.AddAsync(newProduct);
                    await _context.SaveChangesAsync();

                    if (productToDuplicate.WorkInstructions != null)
                    {
                        foreach (var workInstruction in productToDuplicate.WorkInstructions)
                        {
                            var associatedWorkInstruction = await _context.WorkInstructions
                                .AsNoTracking()
                                .FirstOrDefaultAsync(wi => wi.Id == workInstruction.Id);
                            if (associatedWorkInstruction != null)
                            {
                                newProduct.WorkInstructions.Add(associatedWorkInstruction);
                                associatedWorkInstruction.Products.Add(newProduct);
                            }
                            
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    Log.Information("Product: {ProductName} Duplicated Successfully", newProduct.Name);
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    Log.Information("Exception caught while trying to duplicate Product: {ExceptionType}", e.GetType());
                }
            });
        }
        catch (Exception e)
        {
            Log.Information("Exception caught while trying to duplicate Product: {ExceptionType}", e.GetType());
        }
    }

    public async Task AddProductAsync(Product product)
    {
        try
        {
            if (product.WorkInstructions != null)
            {
                product.WorkInstructions = await _context.WorkInstructions
                    .Where(wi => wi.Products.Contains(product))
                    .ToListAsync();
            }
            
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            
            Log.Information("Product successfully created. ID: {ProductID}", product.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occured while adding product");
        }
    }
    
    public async Task<Product?> FindProductByIdAsync(int id)
    {
        try
        {
            var product = await _context.Products
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

    
    public async Task<Product?> FindByTitleAsync(string title)
    {
        try
        {
            var product = await _context.Products
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

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.WorkInstructions)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occured while getting all products. Returning empty product list.");
            return new List<Product>();
        }
    }

    public async Task ModifyProductAsync(Product product)
    {
        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception occured while modifying product. Product ID: {inputId}", product.Id);
        }
        
    }

    public async Task RemoveProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            
            Log.Information("Product successfully removed. ID: {ProductId}", product.Id);
        }
        else
        {
            Log.Warning("Product for removal not found. ID: {InputId}", id);
        }
    }
}