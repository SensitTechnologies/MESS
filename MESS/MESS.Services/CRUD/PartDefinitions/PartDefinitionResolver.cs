using MESS.Data.Context;
using MESS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MESS.Services.CRUD.PartDefinitions;

///<inheritdoc/>
public class PartDefinitionResolver : IPartDefinitionResolver
{
    ///<inheritdoc/>
    public async Task<PartDefinition?> ResolveAsync(
        ApplicationContext context,
        string? name,
        string? number)
    {
        // Normalize input
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
            return null;

        var normalizedNumber = number?.Trim() ?? string.Empty;

        // Use uppercase for comparison to avoid case conflicts
        var upperName = normalizedName.ToUpperInvariant();
        var upperNumber = normalizedNumber.ToUpperInvariant();

        var existing = context.PartDefinitions
                           .Local
                           .FirstOrDefault(p =>
                               p.Name.ToUpper() == upperName &&
                               (p.Number ?? "").ToUpper() == upperNumber)
                       ?? await context.PartDefinitions
                           .FirstOrDefaultAsync(p =>
                               p.Name.ToUpper() == upperName &&
                               (p.Number ?? "").ToUpper() == upperNumber);

        if (existing != null)
            return existing;

        var newPart = new PartDefinition
        {
            Name = normalizedName,
            Number = normalizedNumber
        };

        context.PartDefinitions.Add(newPart);
        return newPart;
    }
}