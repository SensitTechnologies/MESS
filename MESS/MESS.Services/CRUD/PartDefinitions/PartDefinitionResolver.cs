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
        var normalizedName = name?.Trim();
        var normalizedNumber = number?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName))
            return null;

        var upperName = normalizedName.ToUpperInvariant();
        var upperNumber = normalizedNumber.ToUpperInvariant();

        var existing = await context.PartDefinitions
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

        await context.PartDefinitions.AddAsync(newPart);

        return newPart;
    }
}