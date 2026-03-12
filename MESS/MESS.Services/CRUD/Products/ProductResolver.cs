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

        var existingPartDefinitions = await context.PartDefinitions
            .Where(p => normalizedNames.Contains(p.Name))
            .ToListAsync();

        var partLookup = existingPartDefinitions
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in normalizedNames)
        {
            if (!partLookup.ContainsKey(name))
            {
                var partDefinition = new PartDefinition
                {
                    Name = name
                };

                await context.PartDefinitions.AddAsync(partDefinition);
                partLookup[name] = partDefinition;
            }
        }

        var partIds = partLookup.Values.Select(p => p.Id).ToList();

        var existingProducts = await context.Products
            .Where(p => partIds.Contains(p.PartDefinitionId))
            .Include(p => p.PartDefinition)
            .ToListAsync();

        var productLookup = existingProducts
            .ToDictionary(p => p.PartDefinition.Name, StringComparer.OrdinalIgnoreCase);

        var results = new List<Product>();

        foreach (var name in normalizedNames)
        {
            if (!productLookup.TryGetValue(name, out var product))
            {
                var partDefinition = partLookup[name];

                product = new Product
                {
                    PartDefinition = partDefinition,
                    IsActive = true
                };

                await context.Products.AddAsync(product);
            }

            results.Add(product);
        }

        return results;
    }
}