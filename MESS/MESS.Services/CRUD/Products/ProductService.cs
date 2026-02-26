using MESS.Data.Context;
using MESS.Services.DTOs.Products.Detail;
using MESS.Services.DTOs.Products.Summary;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MESS.Services.CRUD.Products;

using Data.Models;
/// <inheritdoc />
public class ProductService : IProductService
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// Provides access to the database context factory and in-memory cache
    /// for managing product data and invalidating cached work instruction associations.
    /// </summary>
    /// <param name="contextFactory">Factory for creating instances of the application database context.</param>
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

            // Load the full product with its PartDefinition and WorkInstructions
            var existingProduct = await context.Products
                .Include(p => p.PartDefinition)
                .Include(p => p.WorkInstructions)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productToDuplicate.Id);

            if (existingProduct == null)
            {
                Log.Warning("Product with ID {ProductId} not found for duplication.", productToDuplicate.Id);
                return;
            }

            // Create a new PartDefinition with distinct Number and Name
            var newPartDefinition = new PartDefinition
            {
                Number = existingProduct.PartDefinition.Number + "-COPY",
                Name = existingProduct.PartDefinition.Name + " (Copy)"
            };

            // Create a new Product referencing the new PartDefinition
            var newProduct = new Product
            {
                PartDefinition = newPartDefinition,
                IsActive = existingProduct.IsActive,
                WorkInstructions = new List<WorkInstruction>()
            };

            await context.Products.AddAsync(newProduct);
            await context.SaveChangesAsync();

            // Reuse existing WorkInstructions (not duplicated)
            if (existingProduct.WorkInstructions != null && existingProduct.WorkInstructions.Any())
            {
                foreach (var wi in existingProduct.WorkInstructions)
                {
                    var existingWI = await context.WorkInstructions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(w => w.Id == wi.Id);

                    if (existingWI != null)
                    {
                        newProduct.WorkInstructions!.Add(existingWI);
                    }
                }
            }

            await context.SaveChangesAsync();

            Log.Information(
                "Product (ID: {OldId}) duplicated successfully as new Product (ID: {NewId}) with PartDefinition (Number: {NewNumber})",
                existingProduct.Id, newProduct.Id, newPartDefinition.Number);
        }
        catch (Exception e)
        {
            Log.Warning("Exception caught while duplicating Product: {ExceptionType} - {Message}", e.GetType(), e.Message);
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
    public async Task CreateAsync(ProductDetailDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var partDefinition = await context.PartDefinitions
                .FirstAsync(pd => pd.Id == dto.PartDefinitionId);

            var product = new Product
            {
                PartDefinitionId = partDefinition.Id,
                PartDefinition = partDefinition,
                IsActive = dto.IsActive
            };

            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            Log.Information("Product successfully created from DTO. ID: {ProductID}", product.Id);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error occurred while adding product from DTO");
        }
    }
    
    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var product = await context.Products
                .Include(p => p.PartDefinition)
                .Include(p => p.WorkInstructions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
                Log.Information("Product found. ID: {ProductId}, PartDefinition: {PartNumber} - {PartName}",
                    product.Id, product.PartDefinition.Number, product.PartDefinition.Name);
            else
                Log.Warning("No product found for ID: {ProductId}", id);

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
                .Include(p => p.PartDefinition) // include PartDefinition for the name
                .Include(p => p.WorkInstructions) // optional if you also want instructions
                .FirstOrDefaultAsync(p => p.PartDefinition.Name == title);

            if (product != null)
                Log.Information("Product found. Title: {ProductTitle}, PartNumber: {PartNumber}",
                    product.PartDefinition.Name, product.PartDefinition.Number);
            else
                Log.Warning("No product found for Title: {ProductTitle}", title);

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
                .Include(p => p.PartDefinition)
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
    public async Task<IEnumerable<ProductSummaryDTO>> GetAllSummariesAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load products with PartDefinition (needed for Name and Number)
            var products = await context.Products
                .Include(p => p.PartDefinition)
                .ToListAsync();

            // Map to ProductSummaryDTO using your mapper
            var summaries = products.ToSummaryDTOList();

            return summaries;
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occurred while getting all product summaries. Returning empty list.");
            return new List<ProductSummaryDTO>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductDetailDTO>> GetAllDetailsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load products with PartDefinition and associated WorkInstructions
            var products = await context.Products
                .Include(p => p.PartDefinition)
                .Include(p => p.WorkInstructions) // assuming a navigation property exists
                .ToListAsync();

            // Map entities to ProductDetailDTOs using your mapper
            var details = products.Select(p => p.ToDetailDTO()).ToList();

            return details;
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occurred while getting all product details. Returning empty list.");
            return new List<ProductDetailDTO>();
        }
    }
    
    /// <inheritdoc />
    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load the existing product with PartDefinition and WorkInstructions
            var existingProduct = await context.Products
                .Include(p => p.PartDefinition)
                .Include(p => p.WorkInstructions)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct is null)
            {
                Log.Warning("Product with ID {ProductId} not found during modification.", product.Id);
                return;
            }

            // Update the IsActive flag
            existingProduct.IsActive = product.IsActive;

            // Update PartDefinition fields
            // Load the tracked PartDefinition
            var existingPartDef = await context.PartDefinitions.FindAsync(existingProduct.PartDefinition.Id);
            if (existingPartDef != null)
            {
                existingPartDef.Name = product.PartDefinition.Name;
                existingPartDef.Number = product.PartDefinition.Number;
            }
            

            // Update WorkInstructions
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

            Log.Information("Product (ID: {ProductId}) updated successfully.", product.Id);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Exception occurred while modifying product. Product ID: {ProductId}", product.Id);
        }
    }
    
    /// <inheritdoc />
    public async Task UpdateProductAsync(ProductDetailDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            // Load the existing product with navigation properties
            var existingProduct = await context.Products
                .Include(p => p.PartDefinition)
                .Include(p => p.WorkInstructions)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (existingProduct is null)
            {
                Log.Warning("Product with ID {ProductId} not found during modification.", dto.ProductId);
                return;
            }

            // Update scalar + PartDefinition fields via mapper
            dto.UpdateEntity(existingProduct);

            // Sync WorkInstructions
            existingProduct.WorkInstructions ??= new List<WorkInstruction>();
            existingProduct.WorkInstructions.Clear();

            if (dto.WorkInstructions != null)
            {
                foreach (var wiDto in dto.WorkInstructions)
                {
                    var trackedWi = await context.WorkInstructions
                        .FindAsync(wiDto.Id);

                    if (trackedWi != null)
                    {
                        existingProduct.WorkInstructions.Add(trackedWi);
                    }
                }
            }

            await context.SaveChangesAsync();

            Log.Information("Product (ID: {ProductId}) updated successfully from DTO.", dto.ProductId);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Exception occurred while modifying product. Product ID: {ProductId}", dto.ProductId);
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