using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.Products;


/// <inheritdoc/>
public class ProductResolver : IProductResolver
{
    /// <inheritdoc/>
    public async Task<List<Product>> ResolveProductsAsync(
        ApplicationContext context,
        IEnumerable<string> productNames)
    {
        var normalizedNames = productNames
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedNames.Count == 0)
            return [];

        // Load existing products + part definitions in one query
        var existingProducts = await context.Products
            .Include(p => p.PartDefinition)
            .Where(p => normalizedNames.Contains(p.PartDefinition.Name))
            .ToListAsync();

        var productLookup = existingProducts
            .GroupBy(p => p.PartDefinition.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var results = new List<Product>();

        foreach (var name in normalizedNames)
        {
            if (!productLookup.TryGetValue(name, out var product))
            {
                product = new Product
                {
                    PartDefinition = new PartDefinition { Name = name },
                    IsActive = true
                };

                context.Products.Add(product);
                productLookup[name] = product;
            }

            results.Add(product);
        }

        return results;
    }
}