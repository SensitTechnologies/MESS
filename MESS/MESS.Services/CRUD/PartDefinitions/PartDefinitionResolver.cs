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
        string? number,
        bool isSerialNumberUnique = true)
    {
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
            return null;

        var normalizedNumber = string.IsNullOrWhiteSpace(number)
            ? null
            : number.Trim();

        var upperName = normalizedName.ToUpperInvariant();
        var upperNumber = normalizedNumber?.ToUpperInvariant() ?? "";

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
        {
            if (existing.IsSerialNumberUnique != isSerialNumberUnique)
            {
                throw new InvalidOperationException(
                    $"Part '{existing.Name}' already exists with IsSerialNumberUnique = {existing.IsSerialNumberUnique}, " +
                    $"but attempted to use {isSerialNumberUnique}. This cannot be changed from the Work Instruction editor.");
            }

            return existing;
        }

        var newPart = new PartDefinition
        {
            Name = normalizedName,
            Number = normalizedNumber,
            IsSerialNumberUnique = isSerialNumberUnique
        };

        context.PartDefinitions.Add(newPart);
        return newPart;
    }
}